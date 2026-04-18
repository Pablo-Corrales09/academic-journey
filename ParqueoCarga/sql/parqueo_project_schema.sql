CREATE DATABASE IF NOT EXISTS parqueo_project
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_0900_ai_ci;

USE parqueo_project;

CREATE TABLE prq_automoviles (
    id INT UNSIGNED NOT NULL AUTO_INCREMENT,
    color VARCHAR(25) NOT NULL,
    anio YEAR NOT NULL,
    fabricante VARCHAR(30) NOT NULL,
    tipo ENUM('Sedán', '4x4', 'moto') NOT NULL,
    PRIMARY KEY (id)
) ENGINE = InnoDB
  COMMENT = 'Catalog of automobiles that can enter the parking lots.';

CREATE TABLE prq_parqueo (
    id INT UNSIGNED NOT NULL AUTO_INCREMENT,
    provincia ENUM('San José', 'Heredia', 'Alajuela', 'Cartago', 'Puntarenas', 'Limón', 'Guanacaste') NOT NULL,
    nombre VARCHAR(50) NOT NULL,
    precio_hora DECIMAL(10,2) NOT NULL,
    PRIMARY KEY (id),
    CONSTRAINT chk_prq_parqueo_precio_hora
        CHECK (precio_hora >= 0)
) ENGINE = InnoDB
  COMMENT = 'Catalog of parking lots, including province and hourly rate.';

CREATE TABLE prq_ingreso_automoviles (
    consecutivo INT UNSIGNED NOT NULL AUTO_INCREMENT,
    id_parqueo INT UNSIGNED NOT NULL,
    id_automovil INT UNSIGNED NOT NULL,
    fecha_entrada DATETIME NOT NULL,
    fecha_salida DATETIME NULL,
    PRIMARY KEY (consecutivo),
    INDEX idx_prq_ingreso_automoviles_id_parqueo (id_parqueo),
    INDEX idx_prq_ingreso_automoviles_id_automovil (id_automovil),
    INDEX idx_prq_ingreso_automoviles_fecha_entrada (fecha_entrada),
    CONSTRAINT fk_prq_ingreso_automoviles_parqueo
        FOREIGN KEY (id_parqueo)
        REFERENCES prq_parqueo (id),
    CONSTRAINT fk_prq_ingreso_automoviles_automovil
        FOREIGN KEY (id_automovil)
        REFERENCES prq_automoviles (id),
    CONSTRAINT chk_prq_ingreso_automoviles_fechas
        CHECK (fecha_salida IS NULL OR fecha_salida >= fecha_entrada)
) ENGINE = InnoDB
  COMMENT = 'Transactional log of automobile entries and exits for each parking lot.';

  INSERT INTO prq_automoviles (color, anio, fabricante, tipo)
  VALUES
    ('Rojo', 2019, 'Toyota', 'Sedán'),
    ('Azul', 2021, 'Honda', 'Sedán'),
    ('Negro', 2018, 'Nissan', '4x4'),
    ('Blanco', 2022, 'Yamaha', 'moto'),
    ('Gris', 2020, 'Hyundai', 'Sedán');

  INSERT INTO prq_parqueo (provincia, nombre, precio_hora)
  VALUES
    ('San José', 'Parqueo Central SJ', 1250.00),
    ('Heredia', 'Parqueo Heredia Norte', 1000.00);

  INSERT INTO prq_ingreso_automoviles (id_parqueo, id_automovil, fecha_entrada, fecha_salida)
  VALUES
    (1, 1, '2026-04-01 08:00:00', '2026-04-01 10:30:00'),
    (1, 2, '2026-04-01 09:15:00', '2026-04-01 11:00:00'),
    (2, 3, '2026-04-01 07:45:00', '2026-04-01 12:15:00'),
    (2, 4, '2026-04-02 13:00:00', '2026-04-02 15:20:00'),
    (1, 5, '2026-04-02 14:10:00', '2026-04-02 18:00:00'),
    (1, 1, '2026-04-03 08:30:00', '2026-04-03 09:45:00'),
    (2, 2, '2026-04-03 10:00:00', '2026-04-03 13:10:00'),
    (1, 3, '2026-04-04 11:20:00', '2026-04-04 16:40:00'),
    (2, 5, '2026-04-05 07:00:00', '2026-04-05 09:30:00'),
    (1, 4, '2026-04-06 17:00:00', '2026-04-06 19:00:00'),
    (2, 1, '2026-04-07 06:50:00', '2026-04-07 08:00:00'),
    (1, 2, '2026-04-08 12:00:00', '2026-04-08 14:25:00'),
    (2, 3, '2026-04-16 18:30:00', NULL),
    (1, 5, '2026-04-17 07:10:00', NULL),
    (2, 4, '2026-04-17 09:45:00', NULL);

    SELECT * FROM prq_automoviles;
    SELECT * FROM prq_parqueo;
    SELECT * FROM prq_ingreso_automoviles;