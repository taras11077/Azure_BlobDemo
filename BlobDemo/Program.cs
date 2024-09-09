using Azure.Storage.Blobs.Models;
using BlobDemo;
using Microsoft.Extensions.Configuration;
using static System.Net.Mime.MediaTypeNames;



var config = new ConfigurationBuilder()
		.AddJsonFile("config.json")
		.Build();

string connectionString = config.GetConnectionString("Default") ?? throw new NullReferenceException("Connection string not found");

var blobService = new BlobService(connectionString);

var container = await blobService.GetContainer("temp");

//string path = "C:\\Users\\Master\\Desktop\\Нова папка";

//await blobService.AddBlob(container, Path.Combine(path, "image11.jpg"));
//await blobService.AddBlob(container, Path.Combine(path, "image8.jpg"));
//await blobService.AddBlob(container, Path.Combine(path, "image14.jpg"));
//await blobService.AddBlob(container, Path.Combine(path, "image15.jpg"));

//await blobService.SetBlobAccessTier(container, "image11.jpg", AccessTier.Cold);

//await blobService.SetBlobMetadata(container, "image11.jpg", new Dictionary<string, string>()
//    {
//        { "Name", "DniproGes_11" },
//        { "Location", "Zaporizhzhya" },
//    }
//);


//await blobService.DeleteBlob(container, "image15.jpg");

//await blobService.DisplayBlobs(container);

//await blobService.AddSnapshot(container, "image11.jpg");

//var sas = await blobService.GetSAS(container, "image11.jpg");
//Console.WriteLine(sas);

//await blobService.DownloadBlob(container, "image11.jpg");


//for (int i = 0; i < 5; i++)
//{
//	File.Copy(Path.Combine(path, "image15.jpg"), Path.Combine(path, $"image15{i}.jpg"));
//	await blobService.AddBlob(container, Path.Combine(path, $"image15{i}.jpg"));
//}

//await blobService.DisplayBlobs(container);

//await blobService.DeleteMultipleBlobs(container, Enumerable.Range(0, 5).Select(i => $"image15{i}.jpg"));

//Console.WriteLine(new string('-', 80));
//await blobService.DisplayBlobs(container);




// HW2 

var user = new User("Vasyl", 27);

// відвантаження юзера в блоб
await blobService.UploadObjectAsync<User>(user, container, "userBlob");

// завантаження юзера з блобу
var downloadedUser = await blobService.DownloadObjectAsync(container, "userBlob");

Console.WriteLine($"Name: {((User)downloadedUser).Name}   Age:{((User)downloadedUser).Age}");
