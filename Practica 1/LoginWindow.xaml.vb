Imports System.Collections.Generic
Imports MySql.Data.MySqlClient

Class LoginWindow

    Private failedAttempts As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
    Private lockoutUntil As New Dictionary(Of String, DateTime)(StringComparer.OrdinalIgnoreCase)

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub LoginButton_Click(sender As Object, e As RoutedEventArgs)
        Dim user = UsernameBox.Text.Trim()
        Dim pwd = PasswordBox.Password
        MessageText.Visibility = Visibility.Collapsed

        If String.IsNullOrEmpty(user) Or String.IsNullOrEmpty(pwd) Then
            ' TC04: campos vacíos -> mostrar MessageBox
            MessageBox.Show("Introduzca datos", "Error", MessageBoxButton.OK, MessageBoxImage.Warning)
            Return
        End If

        Try
            Dim auth = Authenticate(user, pwd)
            Select Case auth
                Case AuthResult.Success
                    Dim mw = New MainWindow()
                    mw.SetWelcome(user)
                    mw.Show()
                    Me.Close()
                Case AuthResult.Locked
                    MessageBox.Show("Se ha bloqueado temporalmente al usuario", "Bloqueado", MessageBoxButton.OK, MessageBoxImage.Warning)
                Case AuthResult.WrongPassword
                    MessageBox.Show("Usuario o contraseña incorrectos", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Case AuthResult.UserNotFound
                    MessageBox.Show("Usuario o contraseña incorrectos", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Select
        Catch ex As MySqlException
            MessageBox.Show("No se pudo conectar con MySQL. Compruebe que el servidor está activo y la configuración es correcta.", "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Sub NewUserButton_Click(sender As Object, e As RoutedEventArgs)
        Dim user = UsernameBox.Text.Trim()
        Dim pwd = PasswordBox.Password

        If String.IsNullOrEmpty(user) Or String.IsNullOrEmpty(pwd) Then
            MessageBox.Show("Introduzca un usuario y una contraseña para registrarse", "Error", MessageBoxButton.OK, MessageBoxImage.Warning)
            Return
        End If

        Try
            If DatabaseHelper.UserExists(user) Then
                MessageBox.Show("El usuario ya existe", "Nuevo usuario", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            DatabaseHelper.CreateUser(user, pwd)
            MessageBox.Show("Usuario creado correctamente", "Nuevo usuario", MessageBoxButton.OK, MessageBoxImage.Information)
        Catch ex As MySqlException
            If ex.Number = 1062 Then
                MessageBox.Show("El usuario ya existe", "Nuevo usuario", MessageBoxButton.OK, MessageBoxImage.Warning)
            Else
                MessageBox.Show("No se pudo conectar con MySQL. Compruebe que el servidor está activo y la configuración es correcta.", "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Error)
            End If
        End Try
    End Sub

    Private Sub ExitButton_Click(sender As Object, e As RoutedEventArgs)
        Application.Current.Shutdown()
    End Sub

    Private Function ValidateCredentials(user As String, pwd As String) As Boolean
        Return DatabaseHelper.ValidateCredentials(user, pwd)
    End Function

    Private Enum AuthResult
        Success = 0
        WrongPassword = 1
        UserNotFound = 2
        Locked = 3
    End Enum

    Private Function Authenticate(user As String, pwd As String) As AuthResult
        Dim key = user.ToLowerInvariant()
        If DatabaseHelper.UserExists(user) = False Then
            Return AuthResult.UserNotFound
        End If

        If lockoutUntil.ContainsKey(key) AndAlso DateTime.Now < lockoutUntil(key) Then
            Return AuthResult.Locked
        End If

        If ValidateCredentials(user, pwd) Then
            If failedAttempts.ContainsKey(key) Then failedAttempts(key) = 0
            If lockoutUntil.ContainsKey(key) Then lockoutUntil.Remove(key)
            Return AuthResult.Success
        End If

        If Not failedAttempts.ContainsKey(key) Then failedAttempts(key) = 0
        failedAttempts(key) += 1
        If failedAttempts(key) >= 3 Then
            lockoutUntil(key) = DateTime.Now.AddSeconds(30)
            failedAttempts(key) = 0
            Return AuthResult.Locked
        End If

        Return AuthResult.WrongPassword
    End Function

    Private Sub ShowMessage(msg As String)
        MessageText.Text = msg
        MessageText.Visibility = Visibility.Visible
    End Sub

End Class
