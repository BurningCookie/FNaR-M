using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class Button : NetworkBehaviour
{
    public Material firstMaterial;
    public Material secondMaterial;

    private NetworkVariable<bool> isOn = new NetworkVariable<bool>(false);
    private Renderer rend;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        isOn.OnValueChanged += OnStateChanged;
        OnStateChanged(false, isOn.Value); // Initial setzen
    }

    private void OnStateChanged(bool previousValue, bool newValue)
    {
        rend.material = newValue ? secondMaterial : firstMaterial;
    }

    public void Interact()
    {
        ActivateButtonServerRpc();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void ActivateButtonServerRpc()
    {
        activateButton();
    }

    private void activateButton()
    {
        if (!isOn.Value)
        {
            isOn.Value = true;
            StartCoroutine(TurnOffAfterDelay());
        }
    }

    private IEnumerator TurnOffAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        isOn.Value = false;
    }
}
