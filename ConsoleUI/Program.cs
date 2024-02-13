using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProcessadorTarefas.Entidades;
using ProcessadorTarefas.Repositorios;
using ProcessadorTarefas.Servicos;
using System.Threading.Tasks;

namespace ConsoleUI
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var visualizacaoTarefas = VisualizacaoTarefas.Nenhuma;

            //Receiving Services for DI
            var serviceProvider = ServicesConfiguration();


            //Instanciando os serviços pela interface
            var processadorTarefas = serviceProvider.GetService<IProcessadorTarefas>();
            var gerenciadorTarefas = serviceProvider.GetService<IGerenciadorTarefas>();
            var configuracao = serviceProvider.GetService<IConfiguration>();

            processadorTarefas.Iniciar();

            while (true)
            {
                if (Console.KeyAvailable)
                {
                    if (int.TryParse(Console.ReadLine(), out int opcao))

                        switch (opcao)
                        {
                            case 1:
                                await gerenciadorTarefas.Criar();
                                break;
                            case 2:
                                Console.WriteLine("Digite o id da tarefa que deseja cancelar:");
                                if (int.TryParse(Console.ReadLine(), out int idTarefa))
                                {
                                    await gerenciadorTarefas.Cancelar(idTarefa);
                                }
                                break;

                            case 3:
                                visualizacaoTarefas = VisualizacaoTarefas.TarefasAtivas;
                                break;

                            case 4:
                                visualizacaoTarefas = VisualizacaoTarefas.TarefasInativas;
                                break;

                                //case 5:
                                //    await processadorTarefas.Encerrar();
                                //case 6:
                                //    await processadorTarefas.Iniciar();
                                //default:
                        }
                }

                Console.Clear();
                MostrarMenuInicial();

                if (visualizacaoTarefas.Equals(VisualizacaoTarefas.TarefasAtivas))
                {
                    var listaAtivas = await gerenciadorTarefas.ListarAtivas();
                    ImprimirListas(listaAtivas);
                }
                else if (visualizacaoTarefas.Equals(VisualizacaoTarefas.TarefasInativas))
                {
                    var listaInativa = await gerenciadorTarefas.ListarInativas();
                    ImprimirListas(listaInativa);
                }

                await Task.Delay(1000);
            }
        }

        static IServiceProvider ServicesConfiguration()
        {
            string basePath = Path.GetFullPath("appsettings.json").Replace("\\bin\\Debug\\net8.0", "");

            //2-Build a configuration
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile(basePath, optional: false, reloadOnChange: true)
                .Build();

            //1&3-Create a Service Collection for DI
            //& Add the configuration to the service Collection (Dependendy Injection Container)
            var services = new ServiceCollection()
                .AddSingleton<IConfiguration>(configuration)
                .AddScoped<IRepository<Tarefa>, MemoryRepository>()
                //.AddScoped<IRepository<Tarefa>>(_ => new SqliteRepository<Tarefa>(connectionString)) //pra usar no futuro
                .AddSingleton<IProcessadorTarefas, ProcessadorTarefasClasse>()
                .AddScoped<IGerenciadorTarefas>(provider =>
                {
                    var repository = provider.GetService<IRepository<Tarefa>>();
                    var serviceProvider = provider.GetService<IProcessadorTarefas>();
                    return new GerenciadorTarefas(repository, serviceProvider);
                })
                .BuildServiceProvider();

            return services;
        }

        static void MostrarMenuInicial()
        {
            Console.WriteLine("--------------------------------------------");
            Console.WriteLine("+            GERENCIAR TAREFAS             +");
            Console.WriteLine("--------------------------------------------");
            Console.WriteLine("+           Escolha uma opção:             +");
            Console.WriteLine("+      1 - Adicionar tarefa                +");
            Console.WriteLine("+      2 - Cancelar tarefa                 +");
            Console.WriteLine("+      3 - Listar tarafas ativas           +");
            Console.WriteLine("+      4 - Listar tarefas inativas         +");
            Console.WriteLine("+      5 - Sair                            +");
            Console.WriteLine("--------------------------------------------");
        }

        static void ImprimirListas(IEnumerable<Tarefa> tarefas)
        {
            foreach (var tarefa in tarefas)
            {
                Console.WriteLine($"Id: {tarefa.Id} | Estado: {tarefa.Estado} | Iniciada em:{tarefa.IniciadaEm} |" +
                    $" Encerrada em: {tarefa.EncerradaEm} | Subtarefas: {tarefa.SubtarefasPendentes.Count + tarefa.SubtarefasExecutadas.Count}");
            }
        }
    }
}