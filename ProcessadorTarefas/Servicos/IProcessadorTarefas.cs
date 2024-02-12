namespace ProcessadorTarefas.Servicos
{
    public interface IProcessadorTarefas
    {
        Task Iniciar();
        Task CancelarTarefa(int idTarefa);
        Task Encerrar();
    }
}
