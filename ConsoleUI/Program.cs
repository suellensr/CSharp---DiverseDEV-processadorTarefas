using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProcessadorTarefas.Entidades;
using ProcessadorTarefas.Repositorios;
using ProcessadorTarefas.Servicos;

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






        }

        public static IServiceProvider ServicesConfiguration()
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
    }
}