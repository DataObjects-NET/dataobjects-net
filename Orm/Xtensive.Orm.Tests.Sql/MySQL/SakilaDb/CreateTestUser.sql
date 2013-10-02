-- Create MySQL DO User
-- Creates do4test user in mysql to enable the tests to run

use mysql;
create user 'root'@'localhost' identified by 'admin';
grant all privileges on *.* to 'root'@'localhost';
flush privileges;