namespace Cambios
{
    using Cambios.Modelos;
    using System.Collections.Generic;
    using System;
    using System.Windows.Forms;
    using Cambios.Servicos;
    using System.Threading.Tasks;

    public partial class Form1 : Form
    {
        #region atributos

        private List<Rate> Rates;

        private NetworkService networkService;

        private ApiService apiService;

        private DialogService dialogService;

        private DataService dataService;

        #endregion

        public Form1()
        {
            InitializeComponent();
            networkService = new NetworkService();
            apiService = new ApiService();
            dialogService = new DialogService();
            dataService = new DataService();
            LoadRates();
        }

        private async void LoadRates()
        {
            bool load;

            //verificar conecção à internet
            lbl_resultado.Text = "A atualizar taxas...";

            var connection = networkService.CheckConnection();
            if (!connection.IsSuccess)
            {
                LoadLocalRates();
                load = false;
            }
            else
            {
                await LoadApiRates();
                load = true;
            }

            if (Rates.Count == 0)
            {
                lbl_resultado.Text = "Não há ligação á Internet" + Environment.NewLine + 
                                     "e não forma préviamente carregadas as taxas." + Environment.NewLine + 
                                     "Tente novamente mais tarde!";

                lbl_status.Text = "Primeira inicialização deverá ter ligação á Internet";
                return;
            }
            comboBox_origem.DataSource = Rates;
            comboBox_origem.DisplayMember = "Name";
            
            //corrige erro microsoft (combobox andam as duas "ligadas" uma à outra)
            comboBox_destino.BindingContext = new BindingContext();

            comboBox_destino.DataSource = Rates;
            comboBox_destino.DisplayMember = "Name";

            ProgressBar1.Value = 100;
            btnConverter.Enabled = true;
            lbl_resultado.Text = "Taxas carregadas....";


            if (load)
            {
                lbl_status.Text = string.Format($"Taxas carregadas da internet a {DateTime.Now.ToString("dddd, dd MMMM yyyy")}");
            }
            else
            {
                lbl_status.Text = string.Format($"Taxas carregadas da Base de Dados");
            }
        }

        private void LoadLocalRates()
        {
            Rates = dataService.GetData();
        }

        private async Task LoadApiRates()
        {
            ProgressBar1.Value = 0;

            var response = await apiService.GetRates("http://cambios.somee.com", "/api/rates");

            Rates = (List<Rate>)response.Result;

            dataService.DeleteData();

            dataService.SaveData(Rates);

        }

        private void btnConverter_Click(object sender, EventArgs e)
        {
            Converter();
        }

        private void Converter()
        {
            if (string.IsNullOrEmpty(txt_valor.Text))
            {
                dialogService.showMessage("Erro","Insira um valor a converter");
                return;
            }

            decimal valor;

            if (!decimal.TryParse(txt_valor.Text,out valor))
            {
                dialogService.showMessage("Erro de Conversão", "Valor terá que ser númerico");
                return;
            }

            if (comboBox_origem.SelectedItem == null)
            {
                dialogService.showMessage("Erro", "Têm de escolher uma moeda a converter");
                return;
            }

            if (comboBox_destino.SelectedItem == null)
            {
                dialogService.showMessage("Erro", "Têm de escolher uma moeda de destino para converter");
                return;
            }

            var taxaOrigem = (Rate) comboBox_origem.SelectedItem;
            var taxaDestino = (Rate)comboBox_destino.SelectedItem;

            var valorConvertido = valor / (decimal)taxaOrigem.TaxRate * (decimal)taxaDestino.TaxRate;

            lbl_resultado.Text = String.Format("{0}{1:C2} = {2} {3:C2}",taxaOrigem.Code,valor,taxaDestino.Code,valorConvertido);


        }

        private void btnTrocar_Click(object sender, EventArgs e)
        {
           Trocar();
        }

        private void Trocar()
        {
            var aux = comboBox_origem.SelectedItem;
            comboBox_origem.SelectedItem = comboBox_destino.SelectedItem;
            comboBox_destino.SelectedItem = aux;
            Converter();
        }
    }
}
