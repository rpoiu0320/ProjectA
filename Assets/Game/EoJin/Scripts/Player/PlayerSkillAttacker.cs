using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;
using Unity.VisualScripting;
using UnityEngine.Rendering;
using UnityEngine.Events;

public class PlayerSkillAttacker : MonoBehaviour
{
    public Skill skill;
    DataManager data;
    [SerializeField] bool debug;
    [SerializeField] float control; //플레이어 위치에서 얼마나 떨어진 거리에서 어택 범위 발동할 지
    float rangeAmount;
    float angle;
    bool isAttack = false;
    public UnityAction OnPlaySkillAnim;
    public UnityAction<GameObject> OnPlayerAttack;
    [SerializeField] PlayerAim aim;
    [SerializeField] public GameObject mousePosObj;

    public void Awake()
    {
        data = GameObject.FindWithTag("DataManager").GetComponent<DataManager>();
        aim = gameObject.GetComponent<PlayerAim>();
    }

    public void Update()
    {
        if (isAttack)
            ApplyDamage();

        mousePosObj.transform.position = aim.mousepos;
    }

    Coroutine ApplyDamageRoutine;

    public void OnPrimarySkill(InputValue value)
    {
        skill = data.CurCharacter.primarySkill;
        aim.attacksize = skill.rangeAmount;
        isAttack = true;
        ApplyDamageRoutine = StartCoroutine(skillDuration());
    }

    public void OnSecondarySkill (InputValue value)
    {
        skill = data.CurCharacter.secondarySkill;
        aim.attacksize = skill.rangeAmount;
        isAttack = true;
        ApplyDamageRoutine = StartCoroutine(skillDuration());
    }

    public void OnSpecailSkill(InputValue value)
    {
        skill = data.CurCharacter.specialSkill;
        aim.attacksize = skill.rangeAmount;
        isAttack = true;
        ApplyDamageRoutine = StartCoroutine(skillDuration());
    }

    IEnumerator skillDuration()
    {
        yield return new WaitForSeconds(skill.duration);
        isAttack = false;
    }

    public void ApplyDamage()
    {
        if (skill == null)
            return;


        if (skill.range == Skill.Range.Circle)
            angle = 180;
        else if (skill.range == Skill.Range.OneDirection)
            angle = 15;
        else
            angle = 60;

        rangeAmount = skill.rangeAmount;

        
        Collider[] colliders = Physics.OverlapSphere(gameObject.transform.position, skill.rangeAmount);

        foreach (Collider collider in colliders)
        {
            Vector3 playerNMouse = (transform.position - aim.mousepos).normalized;
            Vector3 playerNTarget = (collider.transform.position - transform.position).normalized;
            Vector3 dirTarget = (collider.transform.position - transform.position).normalized;

            if (!(collider.tag == "Player")) //따라서 ball의 tag도 Player여야 함
                continue;

            if (Vector3.Dot(-playerNMouse, playerNTarget) < Mathf.Cos(angle * Mathf.Deg2Rad))
                continue;

            if (collider.isTrigger == true)
                continue;
            
            if (collider.gameObject.layer == 7)
            {
                Debug.Log($"{collider.gameObject.name}에게 Attack");
                aim.Attack();
                continue;
            }

            collider.GetComponent<PlayerGetDamage>().GetDamaged(this.gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!debug)
            return;

        Handles.color = Color.cyan;
        Handles.DrawSolidArc(transform.position, Vector3.up, (aim.mousepos - transform.position).normalized, -angle, rangeAmount);
        Handles.DrawSolidArc(transform.position, Vector3.up, (aim.mousepos - transform.position).normalized, angle, rangeAmount);
    }
}
