using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class SlimeMovement : MonoBehaviourPun, IPunObservable
{
    public Camera cam;
    public LayerMask ground;

    private NavMeshAgent agent;

    private Vector3 networkPosition;
    private Quaternion networkRotation;

    [Header("Interpolation")]
    public float positionLerpSpeed = 15f;
    public float rotationLerpSpeed = 15f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError("NavMeshAgent no encontrado.");
            return;
        }

        if (!PhotonNetwork.IsConnected || photonView.IsMine)
        {
            if (cam == null) cam = Camera.main;
        }
        else
        {
            agent.enabled = false;
        }
    }

    void Update()
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
        if (cam == null) return;

        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, ground))
            {
                agent.SetDestination(hit.point);
            }
        }
    }

    void FixedUpdate()
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                networkPosition,
                positionLerpSpeed * Time.deltaTime
            );

            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                networkRotation,
                rotationLerpSpeed * Time.deltaTime
            );
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}