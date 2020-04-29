using UnityEngine.EventSystems;
using UnityEngine;

public class FastEnter : MonoBehaviour,
		IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField]
	private bool isUp;
	private LetterSelecter myLetter;

	private void Start()
	{
		myLetter = GetComponentInParent<LetterSelecter>();
	}

	private void Enter()
	{
		AudioManager.Instance.Play("Click");
		if (isUp)
		{
			myLetter.NextLetter();
		}
		else
		{
			myLetter.PrevLetter();
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		Invoke("Enter", 0.5f);
		InvokeRepeating("Enter", 1.5f, 0.25f);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		CancelInvoke();
	}
}
