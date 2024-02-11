using Microsoft.Extensions.Configuration;
using ProcessadorTarefas.Entidades;
using ProcessadorTarefas.Repositorios;

namespace ProcessadorTarefas.Servicos
{
    public class ProcessadorTarefasClasse : IProcessadorTarefas<Tarefa,Subtarefa>
    {

        private IRepository<Tarefa> _repositorio;
        private IConfiguration _configuracao;

        public ProcessadorTarefasClasse(IRepository<Tarefa> repositorio, IConfiguration configuracao)
        {
            _repositorio = repositorio;
            _configuracao = configuracao;
        }

        public Task CancelarTarefa(int idTarefa)
        {
            throw new NotImplementedException();
        }
        public Task Encerrar()
        {
            throw new NotImplementedException();
        }

        public async Task Iniciar()
        {
            // Aqui você acessa o valor da configuração "quantidadeTarefasEmParalelo"
            var quantidadeTaskParalelo = int.Parse(_configuracao["quantidadeTarefasEmParalelo"]);
            
            if(quantidadeTaskParalelo > 0)
                await ProcessarTarefasAsync(quantidadeTaskParalelo);

        }

        public Queue<Tarefa> PreencherTarefasAgendadas()
        {
            int quatidadeTarefasAgendadas = int.Parse(_configuracao["quantidadeTarefasAgendadas"]!);
            var tarefasAgendadas = new Queue<Tarefa>();

            while(tarefasAgendadas.Count < quatidadeTarefasAgendadas)
            {
                foreach(Tarefa tarefa in _repositorio.GetAll())
                {
                    if(tarefa.Estado == EstadoTarefa.EmPausa)
                    {
                        tarefasAgendadas.Enqueue(tarefa);
                    }
                }
                foreach (Tarefa tarefa in _repositorio.GetAll())
                {
                    if (tarefa.Estado == EstadoTarefa.Criada)
                    {
                        tarefasAgendadas.Enqueue(tarefa);
                    }
                }
            }

            return tarefasAgendadas;
        }

        public async Task ProcessarTarefasAsync(int tarefasEmParalelo) //recebe do arquivo config
        {
            Queue<Tarefa> filaTarefa = PreencherTarefasAgendadas();

            var tasksEmExecucao = new List<Task>();

            //Quando inicializo o programa, entra aqui
            while (tasksEmExecucao.Count < tarefasEmParalelo)
            {
                Tarefa tarefa = filaTarefa.Dequeue();
                tasksEmExecucao.Add(IniciarTarefaAsync(tarefa));  // 5 Tasks <= Tarefas
            }

            //Quando o programa já está executando e uma tarefa termina, entra aqui
            while (tasksEmExecucao.Count > 0)
            {
                var tarefaConcluida = await Task.WhenAny(tasksEmExecucao);
                tasksEmExecucao.Remove(tarefaConcluida);
                if (filaTarefa.Count > 0)
                {
                    Tarefa tarefa = filaTarefa.Dequeue();
                    tasksEmExecucao.Add(IniciarTarefaAsync(tarefa));
                }
                await Task.WhenAll(tasksEmExecucao); //Se não for aqui, colocar logo abaixo da } a seguir
            }

        }

        public async Task IniciarTarefaAsync(Tarefa tarefa)
        {
            tarefa.Estado = EstadoTarefa.EmExecucao;
            var subtarefasPendentes = tarefa.SubtarefasPendentes;

            foreach (var subtarefa in subtarefasPendentes)
            {
                await IniciarSubtarefasAsync(subtarefa);
                tarefa.SubtarefasExecutadas.Add(subtarefa);
                tarefa.SubtarefasPendentes.Remove(subtarefa);
            }

            tarefa.Estado = EstadoTarefa.Concluida;
            tarefa.EncerradaEm = DateTime.Now;
        }

        public async Task IniciarSubtarefasAsync(Subtarefa subtarefa)
        {
            await Task.Delay(subtarefa.Duracao);
        }

    }
}