using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace KID
{
    public class ModelManager : MonoBehaviour
    {
        private string url = "https://g.ubitus.ai/v1/chat/completions";
        private string key = "d4pHv5n2G3q2vkVMPen3vFMfUJic4huKiQbvMmGLWUVIr/ptUuGnsCx/zVdYmVtdrGPO9//2h8Fbp6HkSQ0/oA==";
        private string role = "你是一位音樂推薦者，請盡量將話題保持在音樂為主";

        [SerializeField, Header("輸入欄位")]
        private TMP_InputField inputField;

        [SerializeField, Header("輸出欄位")]
        private TMP_Text outputText;

        [SerializeField, Header("確認視窗物件")]
        private GameObject confirmWindow;

        [SerializeField, Header("確認訊息文字")]
        private TMP_Text confirmMessageText;

        private string prompt;
        private string pendingYouTubeUrl;

        private void Awake()
        {
            inputField.onEndEdit.AddListener(PlayerInput);
            confirmWindow.SetActive(false);
        }

        private void PlayerInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return;
            prompt = input;
            inputField.text = "";
            StartCoroutine(GetResult());
        }

        private IEnumerator GetResult()
        {
            var data = new
            {
                model = "llama-3.1-70b",
                messages = new[]
                {
                    new { name = "user", content = prompt, role = "user" },
                    new { name = "assistant", content = role, role = "system" }
                },
                stop = new string[] { "<|eot_id|>", "<|end_of_text|>" },
                frequency_penalty = 0,
                max_tokens = 100,
                temperature = 0.2,
                top_p = 0.5,
                top_k = 20,
                stream = false
            };

            string json = JsonConvert.SerializeObject(data);
            byte[] postData = Encoding.UTF8.GetBytes(json);

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(postData);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", "Bearer " + key);

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    outputText.text = $"<color=#f11>錯誤：{request.error}</color>";
                }
                else
                {
                    string responseText = request.downloadHandler.text;
                    JObject jObject = JObject.Parse(responseText);
                    string content = jObject["choices"][0]["message"]["content"].ToString();
                    Interaction(content);
                }
            }
        }

        private void Interaction(string response)
        {
            outputText.text = response;
            print($"<color=#f96>結果：{response}</color>");
            string songName = ExtractSongName(response);
            if (!string.IsNullOrEmpty(songName))
            {
                pendingYouTubeUrl = "https://www.youtube.com/results?search_query=" + WWW.EscapeURL(songName);
                ShowConfirmationWindow(songName);
            }
            else
            {
                Debug.LogWarning("未找到有效的歌曲名稱。");
            }
        }

        private string ExtractSongName(string response)
        {
            int startIndex = response.IndexOf('《') + 1;
            int endIndex = response.IndexOf('》');
            if (startIndex > 0 && endIndex > startIndex)
            {
                return response.Substring(startIndex, endIndex - startIndex);
            }
            return string.Empty;
        }

        private void ShowConfirmationWindow(string songName)
        {
            confirmMessageText.text = $"是否跳轉到 YouTube 搜尋：{songName}?";
            confirmWindow.SetActive(true);
        }

        public void OnConfirmYes()
        {
            Application.OpenURL(pendingYouTubeUrl);
            confirmWindow.SetActive(false);
        }

        public void OnConfirmNo()
        {
            pendingYouTubeUrl = null;
            confirmWindow.SetActive(false);
        }
    }
}
