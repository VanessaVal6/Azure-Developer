using Azure.Identity;
using Azure.Storage.Blobs;

Console.WriteLine("Azure Blob Storage exercise\n");

await ProcessAsync();

Console.WriteLine("\nPress enter to exit the sample application.");
Console.ReadLine();

async Task ProcessAsync()
{
	string accountName = Environment.GetEnvironmentVariable("AZURE_STORAGE_ACCOUNT") ?? "storagevane";

	string connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
	BlobServiceClient blobServiceClient;

	if (!string.IsNullOrEmpty(connectionString))
	{
		blobServiceClient = new BlobServiceClient(connectionString);
	}
	else
	{
		var credential = new DefaultAzureCredential();
		string blobServiceEndpoint = $"https://{accountName}.blob.core.windows.net";
		blobServiceClient = new BlobServiceClient(new Uri(blobServiceEndpoint), credential);
	}

	string containerName = "wtblob" + Guid.NewGuid().ToString("N");
	Console.WriteLine("Creating container: " + containerName);
	BlobContainerClient containerClient = await blobServiceClient.CreateBlobContainerAsync(containerName);

	string localPath = "./data/";
	Directory.CreateDirectory(localPath);

	string fileName = "wtfile" + Guid.NewGuid().ToString("N") + ".txt";
	string localFilePath = Path.Combine(localPath, fileName);

	await File.WriteAllTextAsync(localFilePath, "Felicitaciones, aprobaste por fin Vanessa. A mi si me funcionó!");
	Console.WriteLine("Local file created: " + localFilePath);

	BlobClient blobClient = containerClient.GetBlobClient(fileName);
	Console.WriteLine("Uploading to Blob storage as blob:\n\t{0}", blobClient.Uri);

	await using (FileStream uploadFileStream = File.OpenRead(localFilePath))
	{
		try
		{
			await blobClient.UploadAsync(uploadFileStream, overwrite: true);
		}
		catch (Azure.RequestFailedException ex) when (ex.Status == 403)
		{
			Console.WriteLine("Error: autorización denegada al subir el blob (403).\n");
			Console.WriteLine("Posibles soluciones:");
			Console.WriteLine(" - Exportar AZURE_STORAGE_CONNECTION_STRING con la cadena de conexión de la cuenta de almacenamiento.");
			Console.WriteLine(" - Generar un SAS con permisos de escritura ('w' y 'c') y usarlo en la URL o en la conexión.");
			Console.WriteLine(" - Asignar la identidad (usuario o principal de servicio) el rol 'Storage Blob Data Contributor' sobre la cuenta de almacenamiento.");
			Console.WriteLine($"Detalles del error: {ex.ErrorCode} - {ex.Message}");
			throw;
		}
	}

	Console.WriteLine("Blob uploaded successfully.");

	Console.WriteLine("Listing blobs in container...");
	await foreach (var blobItem in containerClient.GetBlobsAsync())
	{
		Console.WriteLine("\t" + blobItem.Name);
	}

	var content = await blobClient.DownloadContentAsync();
	Console.WriteLine("\nContenido del fichero subido al blob:");
	Console.WriteLine(content.Value.Content.ToString());
}
