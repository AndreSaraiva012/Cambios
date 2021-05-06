namespace Cambios.Servicos
{
    using System.Windows.Forms;


    public class DialogService
    {
            public void showMessage(string title,string message)
            {
                MessageBox.Show(title, message);
            }

    }
}
