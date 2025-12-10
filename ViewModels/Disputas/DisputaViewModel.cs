using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppRpgEtec.Models;
using System.Windows.Input;
using AppRpgEtec.Services.Disputas;
using AppRpgEtec.Services.Personagens;
using AppRpgEtec.Services.PersonagemHabilidades;
using AppRpgEtec.Models.Personagens;

namespace AppRpgEtec.ViewModels.Disputas
{
    public class DisputaViewModel : BaseViewModel
    {
        private PersonagemService pService;
        public ObservableCollection<Models.Personagens.Personagem> PersonagensEncontrados { get; set; }
        public Models.Personagens.Personagem Atacante { get; set; }
        public Models.Personagens.Personagem Oponente { get; set; }
        private DisputaService dServices;
        public Disputa DisputaPersonagens { get; set; }
        private PersonagemHabilidadeService phService;
        public ObservableCollection<Models.PersonagemHabilidade> Habilidades { get; set; }
        private PersonagemHabilidade habilidadeSelecionada;

        public PersonagemHabilidade HabilidadeSelecionada
        {
            get { return habilidadeSelecionada; }
            set
            {
                if (value != null)
                {
                    try
                    {
                        habilidadeSelecionada = value;
                        OnPropertyChanged();
                    }
                    catch (Exception ex)
                    {
                        Application.Current.MainPage.DisplayAlert("Ops", ex.Message, "Ok");
                    }
                }
            }
        }

        private async Task ExecutarDisputaHabilidades()
        {
            try
            {
                DisputaPersonagens = new Disputa();
                DisputaPersonagens.AtacantaId = Atacante.Id;
                DisputaPersonagens.OponenteId = Oponente.Id;
                DisputaPersonagens.HabilidadeId = habilidadeSelecionada.HabilidadeId;
                DisputaPersonagens = await dServices.PostDisputaComHabilidadesAsync(DisputaPersonagens);

                await Application.Current.MainPage
                    .DisplayAlert("Resultado", DisputaPersonagens.Narracao, "Ok");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage
                    .DisplayAlert("Ops", ex.Message + " Detalhes: " + ex.InnerException, "Ok");
            }
        }


        public DisputaViewModel()
        {
            string token = Preferences.Get("UsuarioToken", string.Empty);
            pService = new PersonagemService(token);
            dServices = new DisputaService(token);
            phService = new PersonagemHabilidadeService(token);

            Atacante = new Models.Personagens.Personagem();
            Oponente = new Models.Personagens.Personagem();
            DisputaPersonagens = new Disputa();

            PersonagensEncontrados = new ObservableCollection<Models.Personagens.Personagem>();

            PesquisarPersonagensCommand = new Command<string>(async (string pesquisa) =>
                { await PesquisarPersonagens(pesquisa); });

            DisputaComArmaCommand = new Command(async () => { await ExecutarDisputaArmada(); });

            DisputaComHabilidadeCommand = new Command(async () => { await ExecutarDisputaHabilidades(); });
        }

        public async Task ObterHabilidadesAsync(int personagemId)
        {
            try
            {
                Habilidades = await phService.GetPersonagemHabilidadesAsync(personagemId);
                OnPropertyChanged(nameof(Habilidades));
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage
                    .DisplayAlert("Ops", ex.Message + " Detalhes: " + ex.InnerException, "Ok");
            }
        }

        private async Task ExecutarDisputaArmada()
        {
            try
            {
                DisputaPersonagens = new Disputa();
                DisputaPersonagens.AtacantaId = Atacante.Id;
                DisputaPersonagens.OponenteId = Oponente.Id;
                DisputaPersonagens = await dServices.PostDisputaComArmaAsync(DisputaPersonagens);

                await Application.Current.MainPage
                    .DisplayAlert("Resultado", DisputaPersonagens.Narracao, "Ok");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage
                    .DisplayAlert("Ops", ex.Message + " Detalhes: " + ex.InnerException, "Ok");
            }

            DisputaGeralCommand = new Command(async () => { await ExecutarDisputaGeral(); });
        }

        

        public ICommand DisputaComHabilidadeCommand { get; set; }
        public ICommand PesquisarPersonagensCommand { get; set; }
        public ICommand DisputaComArmaCommand { get; set; }
        public ICommand DisputaGeralCommand { get; set; }

        private async Task ExecutarDisputaGeral()
        {
            try
            {
                ObservableCollection<Personagem> lista = await pService.GetPersonagemsAsync();
                DisputaPersonagens = new Disputa();
                DisputaPersonagens.ListaIdPersonagens = lista.Select(x => x.Id).ToList();

                DisputaPersonagens = await dServices.PostDisputaGeralAsync(DisputaPersonagens);

                string resultados = string.Join(" | ", DisputaPersonagens.Resultados);

                await Application.Current.MainPage
                    .DisplayAlert("Resultado", resultados, "Ok");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage
                    .DisplayAlert("Ops", ex.Message + " Detalhes: " + ex.InnerException, "Ok");
            }
        }


        public async Task PesquisarPersonagens(string textoPesquisaPersonagem)
        {
            try
            {
                PersonagensEncontrados = await pService.GetByNomeAproximadoAsync(textoPesquisaPersonagem);
                OnPropertyChanged(nameof(PersonagensEncontrados));
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ops", ex.Message + " Detalhes: " + ex.InnerException, "Ok");
            }
        }

        public string DescricaoPersonagemAtacante
        {
            get => Atacante.Nome;
        }

        public string DescricaoPersonagemOponente
        {
            get => Oponente.Nome;
        }

        public async void SelecionarPersonagem(Models.Personagens.Personagem p)
        {
            try
            {
                string tipoCombatente = await Application.Current.MainPage.DisplayActionSheet("Atacante ou Oponente?",
                    "Cancelar?", "", "Atacante", "Oponente");

                if (tipoCombatente == "Atacante")
                {
                    await this.ObterHabilidadesAsync(p.Id);
                    Atacante = p;
                    OnPropertyChanged(nameof(DescricaoPersonagemAtacante));
                }
                else if (tipoCombatente == "Oponente")
                {
                    Oponente = p;
                    OnPropertyChanged(nameof(DescricaoPersonagemOponente));
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ops", ex.Message + " Detalhes: " + ex.InnerException, "Ok");
            }
        }

        private Models.Personagens.Personagem personagemSelecionado;

        public Models.Personagens.Personagem PersonagemSelecionado
        {
            set
            {
                if (value != null)
                {
                    personagemSelecionado = value;
                    SelecionarPersonagem(personagemSelecionado);
                    OnPropertyChanged();
                    PersonagensEncontrados.Clear();
                }
            }
        }

        private string textoBuscaDigitado = string.Empty;

        public string TextoBuscaDigitado
        {
            get { return textoBuscaDigitado; }
            set
            {
                if ((value != null) && !string.IsNullOrEmpty(value) && value.Length > 0)
                {
                    textoBuscaDigitado = value;
                    _ = PesquisarPersonagens(textoBuscaDigitado);
                }
                else
                {
                    PersonagensEncontrados.Clear();
                }
            }
        }
    }
}
