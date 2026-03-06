using System.ComponentModel;
using EmpadronamientoBackend.Application.DTOs.Requests;

public class RegisterExternalAdminRequest : RegisterRequest
{
    [DefaultValue(2)] // ID de la empresa cliente
    public required int TargetOrganizacionId { get; set; }
}