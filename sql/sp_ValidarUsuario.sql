-- =============================================
-- Author:		IrisSevenSoft
-- Create date: 11-07-2025, 10:15
-- Description:	SP para Validar Login
-- =============================================
CREATE PROCEDURE sp_ValidarUsuario 
	-- Add the parameters for the stored procedure here
	@RunInstalador VARCHAR(20),
	@UsernameInstalador VARCHAR(50),
	@ContrasenaInstalador VARCHAR(255)
AS
BEGIN
	SET NOCOUNT ON;
    -- Insert statements for procedure here
	SELECT 
		i.RunInstalador,
		i.UsernameInstalador,
		i.NombresInstalador,
		i.ApellidosInstalador,
		i.IdRolInstalador,
		r.NombreRol
	FROM Instaladores i
	INNER JOIN Roles r ON i.IdRolInstalador = r.IdRol
	WHERE (i.RunInstalador = @RunInstalador OR i.UsernameInstalador = @UsernameInstalador)
		AND i.ContrasenaInstalador = @ContrasenaInstalador
END
