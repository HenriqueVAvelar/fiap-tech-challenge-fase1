using Oficina.Domain.ValueObjects;

namespace Oficina.Domain.Entities;

public class Veiculo
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public Placa Placa { get; set; }
    public string Marca { get; set; }
    public string Modelo { get; set; }
    public int Ano { get; set; }

    public Cliente? Cliente { get; private set; }

    public Veiculo(int clienteId, Placa placa, string marca, string modelo, int ano)
    {
        ClienteId = clienteId;
        Placa = placa;
        Marca = marca;
        Modelo = modelo;
        Ano = ano;
    }
}
