using UnityEngine;

public class OpenYouTube : MonoBehaviour
{
    // 呼叫此方法來搜尋音樂
    public void SearchYouTube(string query)
    {
        string url = "https://www.youtube.com/results?search_query=" + WWW.EscapeURL(query);
        Application.OpenURL(url);
        Debug.Log("開啟網址：" + url);
    }
}
