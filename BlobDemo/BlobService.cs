using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using System.Text.Json;
using System.Text;
using System.IO;
using Azure.Storage.Blobs.Specialized;

namespace BlobDemo;

internal class BlobService
{
	private readonly string _connectionsString;

	public BlobService(string connectionsString)
	{
		_connectionsString = connectionsString;
	}

	// створення контейнера
	public async Task<BlobContainerClient> GetContainer(string name)
	{
		BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionsString);
		BlobContainerClient container = blobServiceClient.GetBlobContainerClient(name);
		await container.CreateIfNotExistsAsync();

		return container;
	}

	// створення блоба
	public async Task AddBlob(BlobContainerClient container, string path)
	{
		var name = Path.GetFileName(path);
		BlobClient blobClient = container.GetBlobClient(name);

		if (!File.Exists(path))
		{
			throw new FileNotFoundException("file not found");
		}

		await blobClient.UploadAsync(path);
		Console.WriteLine($"Blob '{name}' was uploaded to Azure");
	}

	// показати всі блоби контейнера
	public async Task DisplayBlobs(BlobContainerClient container)
	{
		Console.WriteLine("Name\t\t\tLast Modified\t\tAccess tier\t\tSize");

		await foreach (var blob in container.GetBlobsAsync())
		{
			double size = blob.Properties.ContentLength!.Value / 1024.0;
			Console.WriteLine($"{blob.Name}\t\t{blob.Properties.LastModified!.Value.DateTime.ToShortTimeString()}\t\t\t{blob.Properties.AccessTier}\t\t\t{size.ToString("F2")} KiB");
		}
	}

	// встановлення Access tier блобу
	public async Task SetBlobAccessTier(BlobContainerClient container, string name, AccessTier accessTier)
	{
		BlobClient blobClient = container.GetBlobClient(name);
		await blobClient.SetAccessTierAsync(accessTier);
	}

	// встановлення метаданих блобу
	public async Task SetBlobMetadata(BlobContainerClient container, string name, IDictionary<string, string> metadata)
	{
		BlobClient blobClient = container.GetBlobClient(name);
		await blobClient.SetMetadataAsync(metadata, null, default);
	}


	// видалення блоба
	public async Task DeleteBlob(BlobContainerClient container, string name)
	{
		BlobClient blobClient = container.GetBlobClient(name);
		await blobClient.DeleteIfExistsAsync();
	}



	// додавання снеп-шота блобу
	public async Task AddSnapshot(BlobContainerClient container, string name)
	{
		BlobClient blobClient = container.GetBlobClient(name);

		await blobClient.CreateSnapshotAsync();
	}


	// генерація SAS посилання
	public async Task<string> GetSAS(BlobContainerClient container, string name)
	{
		BlobClient blobClient = container.GetBlobClient(name);

		if (!blobClient.CanGenerateSasUri)
		{
			throw new ArgumentException("blob cannot generate sas");
		}

		BlobSasBuilder builder = new BlobSasBuilder()
		{
			BlobContainerName = container.Name,
			BlobName = name,
			Resource = "b",
			ExpiresOn = DateTime.UtcNow.AddMinutes(10),
		};

		builder.SetPermissions(BlobAccountSasPermissions.Read | BlobAccountSasPermissions.Write);

		Uri uri = blobClient.GenerateSasUri(builder);

		return uri.ToString();
	}


	// скачати блоб в папку
	public async Task DownloadBlob(BlobContainerClient container, string name)
	{
		BlobClient blobClient = container.GetBlobClient(name);

		if (!Directory.Exists("data"))
		{
			Directory.CreateDirectory("data");
		}

		await blobClient.DownloadToAsync(Path.Combine("data", name));
	}

	// множинне видалення блобів
	public async Task DeleteMultipleBlobs(BlobContainerClient container, IEnumerable<string> names)
	{
		BlobBatchClient blobBatchClient = container.GetBlobBatchClient();

		var count = names.Count();

		List<Uri> uris = new List<Uri>(capacity: count);

		foreach (string name in names)
		{
			var blob = container.GetBlobClient(name);
			uris.Add(blob.Uri);
		}

		await blobBatchClient.DeleteBlobsAsync(uris);
	}



	// HW_2
	// додавання у блоби об'екта будь-якого класу
	public async Task UploadObjectAsync<T>(T obj, BlobContainerClient container, string name)
	{

		BlobClient blobClient = container.GetBlobClient(name);


		// серіалізація в JSON
		var jsonString = JsonSerializer.Serialize(obj);

		using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString)))
		{
			await blobClient.UploadAsync(stream, overwrite: true);
		}


		var metadata = new Dictionary<string, string>
		{
			{ "ObjectType", typeof(T).AssemblyQualifiedName } // запис повного імені типу в метадані
		};

		await blobClient.SetMetadataAsync(metadata, null, default);
	}


	// зчитування з блобів об'екта будь-якого класу
	public async Task<object> DownloadObjectAsync(BlobContainerClient container, string name)
	{
		BlobClient blobClient = container.GetBlobClient(name);

		var response = await blobClient.DownloadAsync();

		using var streamReader = new StreamReader(response.Value.Content);
		string jsonString = await streamReader.ReadToEndAsync();

		// читання метаданих
		var metadata = await blobClient.GetPropertiesAsync();
		var objectTypeMetadata = metadata.Value.Metadata["ObjectType"];
		var objectType = Type.GetType(objectTypeMetadata);

		// десеріалізація в початковий тип
		return JsonSerializer.Deserialize(jsonString, objectType);
	}




}

