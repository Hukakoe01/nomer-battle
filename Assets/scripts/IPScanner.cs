using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class IPScanner : MonoBehaviour
{
    public string port = "3000";
    public Action<string> OnServerFound;

    public void StartScan()
    {
        Debug.Log("Старт сканирования IP...");
        StartCoroutine(ScanSubnet());
    }

    private IEnumerator ScanSubnet()
    {
        string localIP = GetLocalIPAddress();
        if (localIP == null)
        {
            Debug.LogWarning("Не удалось определить локальный IP.");
            yield break;
        }

        string subnet = localIP.Substring(0, localIP.LastIndexOf('.') + 1);

        for (int i = 1; i <= 254; i++)
        {
            string ip = subnet + i;
            Debug.Log("Проверка IP: " + ip);

            string url = $"http://{ip}:{port}/state";
            UnityWebRequest request = UnityWebRequest.Get(url);
            request.timeout = 1; // подстраховка, основной таймаут вручную

            // Запускаем ручной таймаут
            Coroutine timeoutCoroutine = StartCoroutine(TimeoutCoroutine(request, 0.25f)); // 250 мс

            yield return request.SendWebRequest();
            StopCoroutine(timeoutCoroutine); // остановить таймер, если успели

            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                if (response.Contains("\"players\""))
                {
                    Debug.Log("Сервер найден на IP: " + ip);
                    OnServerFound?.Invoke(ip);
                    yield break;
                }
                else
                {
                    Debug.Log("Ответ получен, но это не сервер игры: " + ip);
                }
            }
            else
            {
                Debug.Log($"Нет ответа от {ip}: {request.result} | {request.error}");
            }

            // Без задержки — сразу следующая итерация
        }

        Debug.LogWarning("Сервер не найден в сети.");
    }

    private IEnumerator TimeoutCoroutine(UnityWebRequest request, float timeoutSeconds)
    {
        float timer = 0f;

        while (!request.isDone)
        {
            timer += Time.deltaTime;

            if (timer >= timeoutSeconds)
            {
                Debug.Log("Таймаут. Прерываем запрос на: " + request.url);
                request.Abort();
                yield break;
            }

            yield return null;
        }
    }

    private string GetLocalIPAddress()
    {
        var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        return null;
    }
}   