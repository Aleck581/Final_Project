using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace KID
{
    /// <summary>
    /// 模型管理器：互動 AI 人物
    /// </summary>
    public class ModelManager : MonoBehaviour
    {
        /// <summary>
        /// 模型連結
        /// </summary>
        private string url = "https://g.ubitus.ai/v1/chat/completions";
        /// <summary>
        /// 模型金鑰
        /// </summary>
        private string key = "d4pHv5n2G3q2vkVMPen3vFMfUJic4huKiQbvMmGLWUVIr/ptUuGnsCx/zVdYmVtdrGPO9//2h8Fbp6HkSQ0/oA==";
        /// <summary>
        /// 角色設定
        /// </summary>
        private string role = "你是一位音樂推薦者";

        #region 模型處理區域
        [SerializeField, Header("輸入欄位")]
        private TMP_InputField inputField;

        [SerializeField, Header("輸出欄位")]
        private TMP_Text outputText;

        private string prompt;

        private void Awake()
        {
            inputField.onEndEdit.AddListener(PlayerInput);
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
        #endregion

        /// <summary>
        /// 處理互動回應
        /// </summary>
        /// <param name="response">模型回傳的文字結果</param>
        private void Interaction(string response)
        {
            outputText.text = response;
            print($"<color=#f96>結果：{response}</color>");
        }
    }
}
