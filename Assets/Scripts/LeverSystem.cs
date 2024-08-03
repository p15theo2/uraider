using UnityEngine;

public class LeverSystem : MonoBehaviour {
	public Transform charPoint;
	public string charAnim;
	public LeverTarget target;
	public bool camLock;
	public Transform camPoint;
	public GameObject cam;
    public float animTime1;
    public float animTime2;
	public string leverAnim;
    private PlayerController playerController;
	private Animator animator;

	void Start()
	{
		animator = GetComponent<Animator>();
		if (animator == null)
		{
			animator = GetComponentInChildren<Animator>();
		}
	}
    // Use this for initialization
    void OnTriggerStay (Collider other) {
		if (other.gameObject.CompareTag("Player"))
		{
			PlayerInput playerInput = other.gameObject.GetComponent<PlayerInput>();
			if (Input.GetKeyDown(playerInput.action) && other.gameObject.GetComponent<PlayerController>().Anim.GetCurrentAnimatorStateInfo(0).IsName("Idle")) {
                other.gameObject.transform.position = charPoint.position;
                other.gameObject.transform.rotation = charPoint.rotation;
                playerController = other.gameObject.GetComponent<PlayerController>();
				playerController.Lock(camLock? animTime1 + animTime2 : animTime1);
                playerController.Anim.SetTrigger(charAnim);
				animator.SetTrigger(leverAnim);
				Invoke("Action", 5f);
			}
		}
	}

	private void UnLockCam()
	{
		cam.SetActive(false);
	}

	private void Action()
	{
		target.Action();
        if (camLock)
        {
            cam.transform.position = camPoint.position;
            cam.transform.rotation = camPoint.rotation;
            cam.SetActive(true);
            Invoke("UnLockCam", animTime2);
        }
    }
}
