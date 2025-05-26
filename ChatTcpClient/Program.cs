using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using ChatTcpClient;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;

TcpClient client = new();
await client.ConnectAsync("127.0.0.1", 2004);
using NetworkStream stream = client.GetStream();

Console.WriteLine("Введіть ваше ім’я: ");
string name = Console.ReadLine();
Console.WriteLine("Введіть шлях до фото (або залиште порожнім, якщо без фото): ");
string imagePath = Console.ReadLine();
if (imagePath.StartsWith("\"") && imagePath.EndsWith("\""))
    imagePath = imagePath.Substring(1, imagePath.Length - 2);
string fileName = "";
string url = "https://myp22.itstep.click/api/Galleries/upload";
if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath)){
    try{
        byte[] imageBytes = File.ReadAllBytes(imagePath);
        string Imgbase64 = Convert.ToBase64String(imageBytes);

        var json = System.Text.Json.JsonSerializer.Serialize(new { Photo = Imgbase64 });
        using var httpClient = new HttpClient();
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(url, content);

        var result = await response.Content.ReadAsStringAsync();
        var resultObject = JsonConvert.DeserializeObject<ServerResultImage>(result);

        if (response.IsSuccessStatusCode){
            fileName = resultObject.Image;
            Console.WriteLine($"Фото успішно завантажено! Нова назва: http://myp22.itstep.click/images/{fileName}");
        }
        else{
            Console.WriteLine($"Помилка: {response.StatusCode}");
        }
    }
    catch (Exception ex){
        Console.WriteLine($"Виникла помилка: {ex.Message}");
    }
}
await stream.WriteAsync(Encoding.UTF8.GetBytes($"{name} {fileName}"));
Console.WriteLine("ps: Введіть 'photo', щоб переглянути вашее фото, 'update' - змінити фото, 'exit' для виходу з чату або 'photo [ім'я] для перегляду фото іншого користувача");
_ = Task.Run(async () =>
{
    byte[] buffer = new byte[1024];
    while (true){
        int bytes = await stream.ReadAsync(buffer, 0, buffer.Length);
        if (bytes == 0) break;
        Console.WriteLine(Encoding.UTF8.GetString(buffer, 0, bytes));
    }
});
while (true){
    string text = Console.ReadLine();
    if (string.IsNullOrEmpty(text)){
        await stream.WriteAsync(Encoding.UTF8.GetBytes("exit"));
        break;
    }
    if (text.ToLower() == "exit"){
        await stream.WriteAsync(Encoding.UTF8.GetBytes("exit"));
        break;
    }
    if (text.ToLower() == "photo") {
        Console.WriteLine($"Ваше фото збережено як: http://myp22.itstep.click/images/{fileName}"); 
        continue; 
    }
    if (text.StartsWith("photo ")){
        string targetUser = text.Substring(6).Trim();
        await stream.WriteAsync(Encoding.UTF8.GetBytes($"photo {targetUser}"));
        Task.Run(async () =>
        {
            byte[] buffer = new byte[1024];
            int bytesReceived = await stream.ReadAsync(buffer, 0, buffer.Length);
            string photoResponse = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
            Console.WriteLine(photoResponse);
        });
        continue;
    }
    else if (text.ToLower() == "update") {
        Console.WriteLine("Введіть новий шлях до фото: ");
        string newPhotoPath = Console.ReadLine();
        if (newPhotoPath.StartsWith("\"") && newPhotoPath.EndsWith("\""))
            newPhotoPath = newPhotoPath.Substring(1, newPhotoPath.Length - 2);
        if (!string.IsNullOrEmpty(newPhotoPath) && File.Exists(newPhotoPath)) { 
            using var httpClient = new HttpClient(); 
            byte[] imageBytes = File.ReadAllBytes(newPhotoPath); 
            string Imgbase64 = Convert.ToBase64String(imageBytes);
            var json = System.Text.Json.JsonSerializer.Serialize(new { Photo = Imgbase64 }); 
            var content = new StringContent(json, Encoding.UTF8, "application/json"); 
            var response = await httpClient.PostAsync(url, content);
            var result = await response.Content.ReadAsStringAsync();
            var resultObject = JsonConvert.DeserializeObject<ServerResultImage>(result);
            if (response.IsSuccessStatusCode) { 
                fileName = resultObject.Image;
                Console.WriteLine($"Фото успішно оновлено! Нова назва: http://myp22.itstep.click/images/{fileName}");
                await stream.WriteAsync(Encoding.UTF8.GetBytes($"update {name} {fileName}"));
                
            }
            else {
                Console.WriteLine($"Помилка: {response.StatusCode}"); 
            }
        }
        else {
            Console.WriteLine("Невірний шлях до фото!");
        }
        continue;
    }
    byte[] data = Encoding.UTF8.GetBytes(text);
    await stream.WriteAsync(data);
}
client.Close();