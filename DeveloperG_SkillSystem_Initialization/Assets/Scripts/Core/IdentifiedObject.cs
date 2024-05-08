using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu]

// ICloneable : 복사를 하기위한 Interface

public class IdentifiedObject : ScriptableObject, ICloneable
{
    #region 1-3-1
    // 강한 결합
    [SerializeField]
    private Category[] categories;
    #endregion

    #region 1-1
    [SerializeField]
    private Sprite icon;
    [SerializeField]
    private int id = -1;
    [SerializeField]
    private string codeName;
    [SerializeField]
    private string displayName;
    [SerializeField]
    private string description;

    public Sprite Icon => icon;
    public int ID => id;
    public string CodeName => codeName;
    public string DisplayName => displayName;
    // 자식 클래스에서 필요에 따라 내용을 수정할수 있도록 virtual로 선언되었음
    public virtual string Description => description;
    #endregion

    #region 1-2
    // ICloneable를 상속받아 반드시 구현해야 하는 함수
    // virtual로 정의되어 자식 클래스에서 필요에 따라 수정할 수 있음
    public virtual object Clone() => Instantiate(this);
    #endregion

    #region 1-3-2
    // Enumerable.Any : 시퀀스에 요소가 하나라도 있는지 또는 특정 조건에 맞는 요소가 있는지 확인합니다.
    public bool HasCategory(Category category)
        => categories.Any(x => x.ID == category.ID);

    public bool HasCategory(string category)
        => categories.Any(x => x == category);
    #endregion
}
