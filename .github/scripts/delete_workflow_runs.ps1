param(
    [Parameter(Mandatory)]
    [string] $repo,
    [Parameter(Mandatory)]
    [string] $token,
    [Parameter(Mandatory)]
    [string] $keepDays
)

$baseUri = "https://api.github.com"
$repoBaseUri = "$baseUri/repos/$repo"
$actionsBaseUri = "$repoBaseUri/actions"
$runsBaseUri = "$actionsBaseUri/runs"

$headers = @{
    Authorization = "Bearer $token"
    Accept        = "application/vnd.github+json"
}

function Get() {
    param(
        [string] $uri
    )
    return Invoke-RestMethod -Uri $uri -Headers $headers -Method Get
}

function DeleteRuns() {
    param (
        [System.Collections.ArrayList] $runIds
    )
    
    foreach ($id in $runIds) {
        Write-Host "Deleting run $id"
        $deleteUri = "$runsBaseUri/$id"
        Invoke-RestMethod -Uri $deleteUri -Headers $headers -Method Delete
    }
}

#
# Gets all workflow runs, finds among them the ones that meet deletion requirements
# (inactive workflow, closed pull-request or older than required days)
# and sorts them into several buckets - "inactive", "pull_request", "workflow_dispatch", "push"
# and "schedule"
#
function GetWorkflowRunsForDeletion()
{
    [OutputType([System.Collections.Generic.Dictionary[System.String, System.Collections.ArrayList]])]
    param (
        [System.Int32] $storeDays
    )

    [System.Collections.Generic.HashSet[int]]$inactiveWorkflows = New-Object System.Collections.Generic.HashSet[int]

    $allWorkflows = Invoke-RestMethod -Uri "$actionsBaseUri/workflows" -Headers $headers -Method Get

    # number of workflows is less then number of runs
    # so more efficient to get them all and find inactive ones
    foreach ($wf in $allWorkflows.workflows) {
        if ($wf.state -ne "active") {
            $inactiveWorkflows.Add($wf.id) | Out-Null;
        }
    }

    [System.Collections.Generic.Dictionary[System.String, System.Collections.ArrayList]]$result = New-Object System.Collections.Generic.Dictionary"[System.String, System.Collections.ArrayList]"

    #keys of the dictionary equal to events of workflows
    $result["pull_request"]      = New-Object System.Collections.ArrayList
    $result["workflow_dispatch"] = New-Object System.Collections.ArrayList
    $result["push"]              = New-Object System.Collections.ArrayList
    $result["schedule"]          = New-Object System.Collections.ArrayList
    #special bucket
    $result["inactive"]          = New-Object System.Collections.ArrayList


    $page = 1 
    $per_page = 100

    do {
        $uri = "$runsBaseUri" + "?page=$page&per_page=$per_page"
        $response = Invoke-RestMethod -Uri $uri -Headers $headers -Method Get
        
        if ($response.total_count -eq 0) {
            return $result
        }

        [System.DateTime]$created = (Get-Date).AddDays(-$storeDays)

        foreach ($run in $response.workflow_runs) {
            if ($result.ContainsKey($run.event) -eq $true) {
                # Doesn't matter how old the run is - if the workflow is inactive
                # put it for deletion
                if ($inactiveWorkflows.Contains($run.workflow_id)) {
                    $result["inactive"].Add($run) | Out-Null
                }
                else {
                    if ($run.event -eq "pull_request") {
                        # for PRs we collect only closed ones
                        if ($run.pull_requests.Count -eq 0) {
                            $result[$run.event].Add($run) | Out-Null
                        }
                    }
                    else {
                        # for the rest, we apply date filter
                        $created_at = [System.DateTime]::Parse($run.created_at)
                        if ($created_at -lt $created) {
                            $result[$run.event].Add($run) | Out-Null
                        }
                    }
                }
            }
        }

        $page++
    } while ($response.workflow_runs.Count -eq $per_page)

    return $result
}

#
# Deletes runs of inactive workflows (of pull_request, push, workflow_dispatch or schedule event)
#
function DeleteRunsOfInactiveWorkflows() {
    param (
        [System.Collections.Generic.Dictionary[System.String, System.Collections.ArrayList]] $groups
    )

    [System.Collections.ArrayList]$runs = $groups["inactive"]

    if ($runs.Count -eq 0) {
        Write-Host "No runs of inactive workflows. Skipping"
        return
    }

    Write-Host "Deleting runs of inactive workflows"
    $run_ids = $runs | Select-Object -ExpandProperty id
    DeleteRuns $run_ids
}

#
# Deletes runs of closed pull-requests
#
function DeleteRunsForClosedPR() {
    param (
        [System.Collections.Generic.Dictionary[System.String, System.Collections.ArrayList]] $groups
    )

    [System.Collections.ArrayList]$runs = $groups["pull_request"]

    if ($runs.Count -eq 0) {
        Write-Host "No runs for closed pull requests. Skipping"
        return
    }

    Write-Host "Deleting runs for closed pull requests"
    $run_ids = $runs | Select-Object -ExpandProperty id
    DeleteRuns $run_ids
}

#
# Deletes runs of 'workflow_dispatch' event marked for deletion
#
function DeleteOldDispatchedRuns() {
    param (
        [System.Collections.Generic.Dictionary[System.String, System.Collections.ArrayList]] $groups
    )

    [System.Collections.ArrayList]$runs = $groups["workflow_dispatch"]

    if ($runs.Count -eq 0) {
        Write-Host "No dispatched runs to delete. Skipping"
        return
    }

    Write-Host "Deleting old runs that were dispatched"
    $run_ids = $runs | Select-Object -ExpandProperty id
    DeleteRuns $run_ids
}

#
# Deletes runs of 'push' event marked for deletion
#
function DeleteOldRunsOnPush() {
    param (
        [System.Collections.Generic.Dictionary[System.String, System.Collections.ArrayList]] $groups
    )

    [System.Collections.ArrayList]$runs = $groups["push"]

    if ($runs.Count -eq 0) {
        Write-Host "No runs triggered by push to delete. Skipping"
        return
    }

    Write-Host "Deleting old runs triggered by push"
    $run_ids = $runs | Select-Object -ExpandProperty id
    DeleteRuns $run_ids
}

#
# Deletes runs of 'schedule' event marked for deletion
#
function DeleteOldScheduledRuns() {
    param (
        [System.Collections.Generic.Dictionary[System.String, System.Collections.ArrayList]] $groups
    )

    [System.Collections.ArrayList]$runs = $groups["schedule"]

    if ($runs.Count -eq 0) {
        Write-Host "No scheduled runs to delete. Skipping"
        return
    }

    Write-Host "Deleting old runs triggered on schedule"
    $run_ids = $runs | Select-Object -ExpandProperty id
    DeleteRuns $run_ids
}

Write-Host "Collecting runs for closed PRs, inactive workflow runs, and other runs older than $keepDays days"

$preserveDays = [System.Int32]::Parse($keepDays)

$runsForDeletion = GetWorkflowRunsForDeletion $preserveDays

if ($runsForDeletion.ToString() -eq "System.Object[]")
{
    # just in case somebody screws up method and forgot to apply out-null to some operation
	# and results of method became array of objects
    $runsForDeletion = $runsForDeletion[$runsForDeletion.Length - 1];
}

DeleteRunsOfInactiveWorkflows $runsForDeletion

DeleteRunsForClosedPR $runsForDeletion

DeleteOldDispatchedRuns $runsForDeletion

DeleteOldRunsOnPush $runsForDeletion

DeleteOldScheduledRuns $runsForDeletion