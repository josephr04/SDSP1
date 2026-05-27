-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Servidor: 127.0.0.1
-- Tiempo de generación: 27-05-2026 a las 19:15:21
-- Versión del servidor: 10.4.32-MariaDB
-- Versión de PHP: 8.2.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Base de datos: `seguridad_software`
--

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `carpetas`
--

CREATE TABLE `carpetas` (
  `id_carpeta` int(11) NOT NULL,
  `id_usuario` int(11) NOT NULL,
  `nombre` varchar(50) NOT NULL,
  `f_modificacion` datetime NOT NULL,
  `tipo` enum('Carpeta de archivos','Documento Word','Archivo PDF','Archivo PNG','Aplicación') NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `carpetas`
--

INSERT INTO `carpetas` (`id_carpeta`, `id_usuario`, `nombre`, `f_modificacion`, `tipo`) VALUES
(7, 5, 'Hola buenas', '2026-05-26 20:39:29', 'Carpeta de archivos'),
(8, 5, 'Probando probador', '2026-05-26 20:39:34', 'Carpeta de archivos'),
(9, 5, 'xd', '2026-05-26 20:39:38', 'Carpeta de archivos'),
(10, 5, '\' OR 1=1 --', '2026-05-26 20:41:12', 'Carpeta de archivos'),
(11, 5, '\"SELECT * from usuarios ', '2026-05-26 20:42:35', 'Carpeta de archivos'),
(12, 5, 'SELECT * from usuarios --', '2026-05-26 20:43:04', 'Carpeta de archivos'),
(13, 5, 'Hola buenas', '2026-05-26 20:45:14', 'Carpeta de archivos'),
(14, 7, 'Mama', '2026-05-26 21:23:35', 'Carpeta de archivos'),
(15, 7, 'Cre', '2026-05-26 21:23:37', 'Carpeta de archivos'),
(16, 7, 'Clea', '2026-05-26 21:23:41', 'Carpeta de archivos'),
(17, 7, 'Maelle', '2026-05-26 21:23:46', 'Carpeta de archivos');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `logs`
--

CREATE TABLE `logs` (
  `id` int(11) NOT NULL,
  `correo` varchar(255) DEFAULT NULL,
  `evento` varchar(50) DEFAULT NULL,
  `descripcion` varchar(255) DEFAULT NULL,
  `fecha` datetime DEFAULT current_timestamp(),
  `ip` varchar(50) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `logs`
--

INSERT INTO `logs` (`id`, `correo`, `evento`, `descripcion`, `fecha`, `ip`) VALUES
(7, 'themoisesespinosa507@gmail.com', 'LOGIN_EXITOSO', 'Acceso exitoso', '2026-05-24 22:00:53', '::1'),
(8, 'themoisesespinosa507@gmail.com', 'LOGIN_EXITOSO', 'Acceso exitoso', '2026-05-24 23:17:16', '::1'),
(9, 'themoisesespinosa507@gmail.com', 'LOGIN_EXITOSO', 'Acceso exitoso', '2026-05-25 00:01:23', '::1'),
(10, 'themoisesespinosa507@gmail.com', 'LOGIN_EXITOSO', 'Acceso exitoso', '2026-05-25 00:59:39', '::1'),
(11, 'themoisesespinosa507@gmail.com', 'LOGIN_EXITOSO', 'Acceso exitoso', '2026-05-25 18:20:17', '::1'),
(12, 'themoisesespinosa507@gmail.com', 'LOGIN_EXITOSO', 'Acceso exitoso', '2026-05-25 18:33:09', '::1'),
(13, 'ramosel18@gmail.com', 'LOGIN_EXITOSO', 'Acceso exitoso', '2026-05-25 18:37:14', '::1'),
(14, 'themoisesespinosa507@gmail.com', 'LOGIN_EXITOSO', 'Acceso exitoso', '2026-05-25 22:47:30', '::1'),
(15, 'themoisesespinosa507@gmail.com', 'LOGIN_EXITOSO', 'Acceso exitoso', '2026-05-25 22:47:37', '::1'),
(16, 'themoisesespinosa507@gmail.com', 'LOGIN_EXITOSO', 'Acceso exitoso', '2026-05-25 22:53:44', '::1'),
(17, 'themoisesespinosa507@gmail.com', 'LOGIN_EXITOSO', 'Acceso exitoso', '2026-05-25 23:00:53', '::1'),
(18, 'themoisesespinosa507@gmail.com', 'LOGIN_EXITOSO', 'Acceso exitoso', '2026-05-25 23:04:10', '::1'),
(19, 'themoisesespinosa507@gmail.com', 'LOGIN_EXITOSO', 'Acceso exitoso', '2026-05-25 23:20:48', '::1'),
(20, 'themoisesespinosa507@gmail.com', 'LOGIN_EXITOSO', 'Acceso exitoso', '2026-05-25 23:22:22', '::1'),
(21, 'themoisesespinosa507@gmail.com', 'LOGIN_EXITOSO', 'Acceso exitoso', '2026-05-26 20:33:34', '::1'),
(22, 'themoisesespinosa507@gmail.com', 'LOGIN_EXITOSO', 'Acceso exitoso', '2026-05-26 20:37:53', '::1'),
(23, 'ramosel18@gmail.com', 'LOGIN_EXITOSO', 'Acceso exitoso', '2026-05-26 20:48:45', '::1'),
(24, 'ramosel18@gmail.com', 'LOGIN_EXITOSO', 'Acceso exitoso', '2026-05-26 20:56:40', '::1'),
(25, 'ramosel18@gmail.com', 'LOGIN_EXITOSO', 'Acceso exitoso', '2026-05-26 20:57:32', '::1'),
(26, 'ramosel18@gmail.com', 'LOGIN_EXITOSO', 'Acceso exitoso', '2026-05-26 21:04:52', '::1'),
(27, 'ramosel18@gmail.com', 'LOGIN_EXITOSO', 'Acceso exitoso', '2026-05-26 21:09:41', '::1'),
(28, 'ramosel18@gmail.com', 'LOGIN_EXITOSO', 'Acceso exitoso', '2026-05-26 21:10:34', '::1'),
(29, 'ramosel18@gmail.com', 'LOGIN_EXITOSO', 'Acceso exitoso', '2026-05-26 21:16:25', '::1'),
(30, 'ramosel18@gmail.com', 'LOGIN_EXITOSO', 'Acceso exitoso', '2026-05-26 21:20:07', '::1'),
(31, 'ramosel18@gmail.com', 'LOGIN_EXITOSO', 'Acceso exitoso', '2026-05-26 21:21:26', '::1'),
(32, 'ramosel18@gmail.com', 'LOGIN_EXITOSO', 'Acceso exitoso', '2026-05-26 21:22:01', '::1'),
(33, 'ramosel18@gmail.com', 'LOGIN_EXITOSO', 'Acceso exitoso', '2026-05-26 21:22:40', '::1'),
(34, 'themoisesespinosa507@gmail.com', 'LOGIN_EXITOSO', 'Acceso exitoso', '2026-05-26 21:24:30', '::1'),
(35, 'themoisesespinosa507@gmail.com', 'LOGIN_EXITOSO', 'Acceso exitoso', '2026-05-26 21:25:22', '::1'),
(36, 'ramosel18@gmail.com', 'LOGIN_EXITOSO', 'Acceso exitoso', '2026-05-26 21:45:48', '::1'),
(37, 'ramosel18@gmail.com', 'LOGIN_EXITOSO', 'Acceso exitoso', '2026-05-26 21:46:15', '::1'),
(38, 'themoisesespinosa507@gmail.com', 'LOGIN_EXITOSO', 'Acceso exitoso', '2026-05-26 21:49:41', '::1'),
(39, 'themoisesespinosa507@gmail.com', 'LOGIN_EXITOSO', 'Acceso exitoso', '2026-05-26 23:29:48', '::1'),
(40, 'micalex1226@gmail.com', 'LOGIN_EXITOSO', 'Acceso exitoso', '2026-05-27 01:11:18', '::1'),
(41, 'micalex1226@gmail.com', 'LOGIN_EXITOSO', 'Acceso exitoso', '2026-05-27 01:15:39', '::1'),
(42, 'micalex1226@gmail.com', 'LOGIN_EXITOSO', 'Acceso exitoso', '2026-05-27 01:16:12', '::1'),
(43, 'themoisesespinosa507@gmail.com', 'LOGIN_EXITOSO', 'Acceso exitoso', '2026-05-27 01:16:30', '::1'),
(44, 'themoisesespinosa507@gmail.com', 'LOGIN_FALLIDO', 'Contraseña incorrecta, intentos restantes: 2', '2026-05-27 01:18:10', '::1'),
(45, 'themoisesespinosa507@gmail.com', 'LOGIN_FALLIDO', 'Contraseña incorrecta, intentos restantes: 1', '2026-05-27 01:18:20', '::1'),
(46, 'micalex1226@gmail.com', 'LOGIN_FALLIDO', 'Contraseña incorrecta, intentos restantes: 2', '2026-05-27 01:18:58', '::1'),
(47, 'themoisesespinosa507@gmail.com', 'LOGIN_EXITOSO', 'Acceso exitoso', '2026-05-27 01:33:00', '::1'),
(48, 'ramosel18@gmail.com', 'LOGIN_FALLIDO', 'Contraseña incorrecta, intentos restantes: 2', '2026-05-27 01:51:07', '::1'),
(49, 'ramosel18@gmail.com', 'LOGIN_FALLIDO', 'Contraseña incorrecta, intentos restantes: 1', '2026-05-27 01:51:29', '::1'),
(50, 'micalex1226@gmail.com', 'LOGIN_FALLIDO', 'Contraseña incorrecta, intentos restantes: 1', '2026-05-27 01:52:02', '::1'),
(51, 'themoisesespinosa507@gmail.com', 'LOGIN_FALLIDO', 'Contraseña incorrecta, intentos restantes: 2', '2026-05-27 01:52:08', '::1'),
(52, 'themoisesespinosa507@gmail.com', 'LOGIN_FALLIDO', 'Contraseña incorrecta, intentos restantes: 1', '2026-05-27 01:52:11', '::1');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `usuarios`
--

CREATE TABLE `usuarios` (
  `id_usuario` int(11) NOT NULL,
  `nombre` varchar(50) NOT NULL,
  `correo` varchar(100) NOT NULL,
  `contraseña` varchar(255) NOT NULL,
  `auth` varchar(6) DEFAULT NULL,
  `intentosFallidos` int(11) DEFAULT 0,
  `bloqueado` int(11) DEFAULT 0,
  `fechaRegistro` datetime DEFAULT current_timestamp(),
  `fechaBloqueo` datetime DEFAULT NULL,
  `two_factor_secret` varchar(255) DEFAULT NULL COMMENT 'Secreto TOTP cifrado (Base32)',
  `two_factor_enabled` varchar(1) NOT NULL DEFAULT '0' COMMENT 'Si 2FA está habilitado',
  `two_factor_verified_at` datetime DEFAULT NULL COMMENT 'Fecha de última verificación exitosa de 2FA'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `usuarios`
--

INSERT INTO `usuarios` (`id_usuario`, `nombre`, `correo`, `contraseña`, `auth`, `intentosFallidos`, `bloqueado`, `fechaRegistro`, `fechaBloqueo`, `two_factor_secret`, `two_factor_enabled`, `two_factor_verified_at`) VALUES
(5, 'Mango', 'themoisesespinosa507@gmail.com', '$2a$11$ItrDtSWVLy5/Te8nZN9p..xfmUc71.noMdI.uj91lXP9/WYsNuWR6', NULL, 2, 0, '2026-05-25 23:04:00', NULL, 'CfDJ8JZoY3uZot1ElNSoitbvkjmRRH_D2OJlPGDzFQMgiwS6X5PRsP11yRLN9oEKLwvEADEBmLEb1yrMohaLOcIE7aHH8w1Nr5Vin18ADflWfrYJUxbw2KLtptnKpvngQybigjNXF5oZAS9GanlRezZP8OQbEtmZf5t9CiI14PTMvmBP', '1', '2026-05-27 01:33:10'),
(7, 'Elvia', 'ramosel18@gmail.com', '$2a$11$A0klOxBBtYX12MwTGnS7t.Zuw3F4s9wt/tT.TMBrD0pLULjhMFOeq', NULL, 2, 0, '2026-05-26 21:09:38', NULL, 'CfDJ8JZoY3uZot1ElNSoitbvkjnG7lQCPhfW_v2Pq0JPub-IkKCrYzDKB79BLOXHSztvzxhlfAkqZR1wySgvqc-hgHGt8tqZevvOUcVuGBcZzowtiANHQVFT76EdVA41apKzo8WtkFacQXM5DfPkg0fCzAv_0TY_LewHV4iVs6JfKDnR', '1', '2026-05-26 21:46:38'),
(9, 'Michalex', 'micalex1226@gmail.com', '$2a$11$FbcoMQDF5dy3GPghuDNVC.LzfhNDwnd6m9guM5jdOW9ewktRLfolq', NULL, 2, 0, '2026-05-27 01:15:29', NULL, 'CfDJ8JZoY3uZot1ElNSoitbvkjkmySNAWHir98LrEw4c64-yierRFUTlGZT7hofuca_PqYSdyHuvg8QTvYz5UN9lA35rXflD1VgegpucgZVYExeLY0tN56nCthoC_e3ZfUTufF2MFqGoeNTYUPWLGLpyDlnujxXtRYIosNVkYYciYC8Y', '1', '2026-05-27 01:16:17');

--
-- Índices para tablas volcadas
--

--
-- Indices de la tabla `carpetas`
--
ALTER TABLE `carpetas`
  ADD PRIMARY KEY (`id_carpeta`),
  ADD KEY `fk_id_usuario` (`id_usuario`);

--
-- Indices de la tabla `logs`
--
ALTER TABLE `logs`
  ADD PRIMARY KEY (`id`);

--
-- Indices de la tabla `usuarios`
--
ALTER TABLE `usuarios`
  ADD PRIMARY KEY (`id_usuario`),
  ADD KEY `idx_two_factor_enabled` (`two_factor_enabled`),
  ADD KEY `idx_correo` (`correo`);

--
-- AUTO_INCREMENT de las tablas volcadas
--

--
-- AUTO_INCREMENT de la tabla `carpetas`
--
ALTER TABLE `carpetas`
  MODIFY `id_carpeta` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=19;

--
-- AUTO_INCREMENT de la tabla `logs`
--
ALTER TABLE `logs`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=53;

--
-- AUTO_INCREMENT de la tabla `usuarios`
--
ALTER TABLE `usuarios`
  MODIFY `id_usuario` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=10;

--
-- Restricciones para tablas volcadas
--

--
-- Filtros para la tabla `carpetas`
--
ALTER TABLE `carpetas`
  ADD CONSTRAINT `fk_id_usuario` FOREIGN KEY (`id_usuario`) REFERENCES `usuarios` (`id_usuario`) ON DELETE CASCADE ON UPDATE CASCADE;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
