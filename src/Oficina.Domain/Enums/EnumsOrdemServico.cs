using System.ComponentModel;

namespace Oficina.Domain.Enums;

public class EnumsOrdemServico
{
    public enum StatusOrdemServico
    {
        [Description("Recebida")]
        Recebida = 1,
        [Description("Em Diagnóstico")]
        EmDiagnostico = 2,
        [Description("Aguardando Aprovação")]
        AguardandoAprovacao = 3,
        [Description("Em Execução")]
        EmExecucao = 4,
        [Description("Finalizada")]
        Finalizada = 5,
        [Description("Entregue")]
        Entregue = 6
    }
}