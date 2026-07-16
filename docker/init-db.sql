-- Create the databases the Stella plugins expect. MARIADB_DATABASE only
-- creates one DB on first boot; the core plugin needs stella_core.
CREATE DATABASE IF NOT EXISTS stella_core;
GRANT ALL PRIVILEGES ON stella_core.* TO 'stella'@'%';
CREATE DATABASE IF NOT EXISTS stella_kfc;
GRANT ALL PRIVILEGES ON stella_kfc.* TO 'stella'@'%';
FLUSH PRIVILEGES;