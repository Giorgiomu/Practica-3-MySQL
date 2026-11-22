-- Base de datos para el catálogo de videojuegos
-- Ejecutar este script con un usuario con permisos para crear bases de datos.

CREATE DATABASE IF NOT EXISTS catalogo_videojuegos
	CHARACTER SET utf8mb4
	COLLATE utf8mb4_unicode_ci;

USE catalogo_videojuegos;

CREATE TABLE IF NOT EXISTS Usuarios (
	id INT NOT NULL AUTO_INCREMENT,
	nombre_usuario VARCHAR(50) NOT NULL,
	password VARCHAR(255) NOT NULL,
	PRIMARY KEY (id),
	UNIQUE KEY UQ_Usuarios_Nombre (nombre_usuario)
);

-- Usuarios de prueba. INSERT IGNORE evita duplicados si el script se ejecuta más de una vez.
INSERT IGNORE INTO Usuarios (nombre_usuario, password)
VALUES
	('admin', 'pass123'),
	('usuario', '1234');
    
USE catalogo_videojuegos;

DELETE FROM Usuarios
WHERE nombre_usuario = 'prueba05';

