Imports MySql.Data.MySqlClient

''' <summary>
''' Centraliza la conexión y las operaciones de usuarios en MySQL.
''' La aplicación obtiene la configuración de variables de entorno para no
''' guardar contraseñas en el repositorio.
''' </summary>
Public NotInheritable Class DatabaseHelper
    Private Sub New()
    End Sub

    Private Shared ReadOnly ConnectionString As String = BuildConnectionString()

    Private Shared Function BuildConnectionString() As String
        Dim builder As New MySqlConnectionStringBuilder With {
            .Server = GetSetting("MYSQL_SERVER", "localhost"),
            .Port = GetPortSetting(),
            .Database = GetSetting("MYSQL_DATABASE", "catalogo_videojuegos"),
            .UserID = GetSetting("MYSQL_USER", "root"),
            .Password = GetSetting("MYSQL_PASSWORD", String.Empty),
            .SslMode = MySqlSslMode.Disabled,
            .ConnectionTimeout = 5
        }

        Return builder.ConnectionString
    End Function

    Private Shared Function GetSetting(name As String, defaultValue As String) As String
        Dim value = Environment.GetEnvironmentVariable(name)
        If String.IsNullOrWhiteSpace(value) Then
            Return defaultValue
        End If

        Return value.Trim()
    End Function

    Private Shared Function GetPortSetting() As UInt32
        Dim value = Environment.GetEnvironmentVariable("MYSQL_PORT")
        Dim port As UInt32

        If UInt32.TryParse(value, port) AndAlso port > 0 Then
            Return port
        End If

        Return 3306UI
    End Function

    Public Shared Function ValidateCredentials(username As String, password As String) As Boolean
        Const sql As String = "SELECT COUNT(*) FROM Usuarios WHERE nombre_usuario = @nombre_usuario AND password = @password;"

        Using connection As New MySqlConnection(ConnectionString)
            connection.Open()

            Using command As New MySqlCommand(sql, connection)
                command.Parameters.Add("@nombre_usuario", MySqlDbType.VarChar, 50).Value = username
                command.Parameters.Add("@password", MySqlDbType.VarChar, 255).Value = password
                Return Convert.ToInt32(command.ExecuteScalar()) > 0
            End Using
        End Using
    End Function

    Public Shared Function UserExists(username As String) As Boolean
        Const sql As String = "SELECT COUNT(*) FROM Usuarios WHERE nombre_usuario = @nombre_usuario;"

        Using connection As New MySqlConnection(ConnectionString)
            connection.Open()

            Using command As New MySqlCommand(sql, connection)
                command.Parameters.Add("@nombre_usuario", MySqlDbType.VarChar, 50).Value = username
                Return Convert.ToInt32(command.ExecuteScalar()) > 0
            End Using
        End Using
    End Function

    Public Shared Sub CreateUser(username As String, password As String)
        Const sql As String = "INSERT INTO Usuarios (nombre_usuario, password) VALUES (@nombre_usuario, @password);"

        Using connection As New MySqlConnection(ConnectionString)
            connection.Open()

            Using command As New MySqlCommand(sql, connection)
                command.Parameters.Add("@nombre_usuario", MySqlDbType.VarChar, 50).Value = username
                command.Parameters.Add("@password", MySqlDbType.VarChar, 255).Value = password
                command.ExecuteNonQuery()
            End Using
        End Using
    End Sub
End Class
