using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System.Collections;

public class NetworkStartUI : MonoBehaviour
{
    private GameObject networkStartUI;
    private Button hostButton;
    private Button clientButton;
    private TMP_InputField ipInputField;
    private TMP_InputField portInputField;
    private TMP_Text invalidInputText;
    private TMP_Text connectionFailedText;

    private void Awake()
    {
        networkStartUI = GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == "NetworkStartUI")?.gameObject;
        hostButton = GetComponentsInChildren<Button>().FirstOrDefault(b => b.name == "HostButton");
        clientButton = GetComponentsInChildren<Button>().FirstOrDefault(b => b.name == "ClientButton");
        ipInputField = GetComponentsInChildren<TMP_InputField>().FirstOrDefault(i => i.name == "EnterIP");
        portInputField = GetComponentsInChildren<TMP_InputField>().FirstOrDefault(i => i.name == "EnterPort");
        invalidInputText = GetComponentsInChildren<TMP_Text>().FirstOrDefault(t => t.name == "InvalidInput");
        connectionFailedText = GetComponentsInChildren<TMP_Text>().FirstOrDefault(t => t.name == "ConnectionFailed");

        invalidInputText.gameObject.SetActive(false);
        connectionFailedText.gameObject.SetActive(false);
    }

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        networkStartUI.SetActive(false);
    }

    public void StartClient()
    {
        if (ipInputField == null || portInputField == null)
        {
            Debug.LogError("InputFields not found!");
            return;
        }

        string ipAddress = ipInputField.text;
        string portText = portInputField.text;

        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager.Singleton is null!");
            return;
        }

        if (!string.IsNullOrEmpty(ipAddress))
        {
            var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
            if (transport == null)
            {
                Debug.LogError("UnityTransport component not found on NetworkManager!");
                return;
            }

            if (ushort.TryParse(portText, out ushort port))
            {
                transport.SetConnectionData(ipAddress, port);
            }
            else
            {
                Debug.LogError("Invalid port number!");
                if (invalidInputText != null)
                {
                    invalidInputText.gameObject.SetActive(true);
                    StartCoroutine(deactivateAfterDelay(invalidInputText, 3f));
                }
                return;
            }
        }

        NetworkManager.Singleton.StartClient();
        networkStartUI.SetActive(false);
        StartCoroutine(CheckConnectionTimeout(5f));
    }

    private IEnumerator CheckConnectionTimeout(float timeout)
    {
        yield return new WaitForSeconds(timeout);
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsConnectedClient)
        {
            NetworkManager.Singleton.Shutdown();
            Debug.LogError("Connection timeout: Unable to connect to the server.");
            networkStartUI.SetActive(true);
            if (connectionFailedText != null)
            {
                connectionFailedText.gameObject.SetActive(true);
                StartCoroutine(deactivateAfterDelay(connectionFailedText, 3f));
            }
        }
    }

    private IEnumerator deactivateAfterDelay(TMP_Text textElement, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (textElement != null)
        {
            textElement.gameObject.SetActive(false);
        }
    }
}
