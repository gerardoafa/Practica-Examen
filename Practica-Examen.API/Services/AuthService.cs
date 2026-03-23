namespace Practica_Examen.API.Services;

using Google.Cloud.Firestore;
using Practica_Examen.API.DTOs;
using Practica_Examen.API.Models;
public class AuthService
{
    private readonly FirebaseService _firebase;
    private readonly JwtService _jwt;

    public AuthService(FirebaseService firebase, JwtService jwt)
    {
        _firebase = firebase;
        _jwt = jwt;
    }

    public async Task<(bool success, string message)> Register(RegisterRequest req)
    {
        // Validar correo único
        var existente = await _firebase.GetCollection("usuarios")
            .WhereEqualTo("email", req.Email)
            .GetSnapshotAsync();

        if (existente.Count > 0)
            return (false, "El correo ya está registrado");

        var hash = BCrypt.Net.BCrypt.HashPassword(req.Password);

        var usuario = new Dictionary<string, object>
        {
            { "nombre", req.Nombre },
            { "email", req.Email },
            { "password", hash },
            { "rol", "usuario" },
            { "activo", true },
            { "fechaRegistro", Timestamp.GetCurrentTimestamp() },
            { "multas", 0.0 }
        };

        await _firebase.GetCollection("usuarios").AddAsync(usuario);
        return (true, "Usuario registrado exitosamente");
    }

    public async Task<(bool success, string message, LoginResponse? data)> Login(LoginRequest req)
    {
        var snapshot = await _firebase.GetCollection("usuarios")
            .WhereEqualTo("email", req.Email)
            .GetSnapshotAsync();

        if (snapshot.Count == 0)
            return (false, "Credenciales inválidas", null);

        var doc = snapshot.Documents[0];
        var usuario = doc.ConvertTo<Usuario>();
        usuario.Id = doc.Id;

        // Verificar cuenta activa
        var activo = doc.ContainsField("activo") && doc.GetValue<bool>("activo");
        if (!activo)
            return (false, "Cuenta inactiva", null);

        if (!BCrypt.Net.BCrypt.Verify(req.Password, usuario.Password))
            return (false, "Credenciales inválidas", null);

        var token = _jwt.GenerarToken(usuario.Id!, usuario.Email, usuario.Rol);

        return (true, "Login exitoso", new LoginResponse
        {
            Token = token,
            UsuarioId = usuario.Id!,
            Nombre = usuario.Nombre,
            Email = usuario.Email,
            Rol = usuario.Rol
        });
    }
}