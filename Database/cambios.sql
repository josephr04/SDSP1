-- Agregar columnas faltantes a usuarios
ALTER TABLE `usuarios`
  ADD COLUMN `intentosFallidos` int(11) DEFAULT 0,
  ADD COLUMN `bloqueado` int(11) DEFAULT 0,
  ADD COLUMN `fechaRegistro` datetime DEFAULT current_timestamp(),
  ADD COLUMN `fechaBloqueo` datetime DEFAULT NULL;

-- Crear tabla logs
CREATE TABLE `logs` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `correo` varchar(255) DEFAULT NULL,
  `evento` varchar(50) DEFAULT NULL,
  `descripcion` varchar(255) DEFAULT NULL,
  `fecha` datetime DEFAULT current_timestamp(),
  `ip` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;