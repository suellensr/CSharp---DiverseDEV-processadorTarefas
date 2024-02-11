namespace ProcessadorTarefas.Servicos
{
    public interface IProcessadorTarefas<T,Y>
    {
        Task Iniciar();
        Task CancelarTarefa(int idTarefa);
        Task Encerrar();
    }
}
