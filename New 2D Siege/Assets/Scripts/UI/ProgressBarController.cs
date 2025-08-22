using System.Collections;
using PurrNet;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarController : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    private Coroutine currentCoroutine;
    private InteractionRequest currentRequest;

    public void BeginInteraction(InteractionRequest request)
    {
        if (currentCoroutine != null) return;
        if (request.canStart != null && !request.canStart()) return;
        
        if (!InstanceHandler.TryGetInstance(out GameController gameController))
        {
            Debug.LogError($"GameStartState failed to get gameController!", this);
        }

        gameController.canMove = false;
        
        currentRequest = request;
        gameObject.SetActive(true);
        currentRequest.onStart?.Invoke();
        currentCoroutine = StartCoroutine(ProgressRoutine(gameController));
        
    }

    private IEnumerator ProgressRoutine(GameController gameController)
    {
        float timer = 0f;
        fillImage.fillAmount = 0f;

        while (timer < currentRequest.duration)
        {
            if (!currentRequest.key.IsPressed())
            {
                Cancel(gameController);
                currentRequest.onCancel?.Invoke();
                yield break;
            }
            
            timer += Time.deltaTime;
            fillImage.fillAmount = timer / currentRequest.duration;
            yield return null;
        }

        gameController.canMove = true;
        currentRequest.onComplete?.Invoke();
        currentCoroutine = null;
        gameObject.SetActive(false);
    }

    public void Cancel(GameController gameController)
    {
        gameController.canMove = true;
        if (currentCoroutine == null)
            return;
        StopCoroutine(currentCoroutine);
        currentCoroutine = null;
        gameObject.SetActive(false);
    }
}
