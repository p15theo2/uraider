using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class Freeclimb : StateBase<PlayerController>
{
    public override void OnEnter(PlayerController entity)
    {
        base.OnEnter(entity);

        entity.Anim.applyRootMotion = true;
        entity.Anim.SetBool("isFreeclimb", true);
    }
    public override void Update(PlayerController entity)
    {
        
    }
}

