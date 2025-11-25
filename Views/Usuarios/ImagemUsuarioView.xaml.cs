using System.ComponentModel;
using AppRpgEtec.Services.Usuarios;
using Azure.Storage.Blobs;
using AppRpgEtec.ViewModels;

namespace AppRpgEtec.Views.Usuarios;

public partial class ImagemUsuarioView : ContentPage
{
	public ImagemUsuarioView()
	{
		string token = Preferences.Get("UsuarioToken", string.Empty);
 		uService = new UsuarioService(token);

		FotografarCommand = new Command(Fotografar);
		SalvarImagemCommand = new Command(SalvarImagemAzure);
		AbrirGaleriaCommand = new Command(AbrirGaleria);

		CarregarUsuarioAzure();
	}

	public async void CarregarUsuarioAzure()
	{
		try
		{
			int usuarioId = Preferences.Get("UsuarioId", 0);
			string filename = $"{usuarioId}.jpg";
			var blobClient = new BlobClient(conexaoAzureStorage, container, filename);

			if (blobClient.Exists())
			{
				Byte[] fileBytes;

				using(MemoryStream ms = new MemoryStream())
				{
					blobClient.OpenRead().CopyTo(ms);
					fileBytes = ms.ToArray();
				}
				Foto = fileBytes;
			}
		}
		catch (Exception ex) 
		{
			await Application.Current.MainPage.DisplayAlert("Ops", ex.Message + " Detalhes: " + ex.InnerException, "Ok");
		}
	}
}