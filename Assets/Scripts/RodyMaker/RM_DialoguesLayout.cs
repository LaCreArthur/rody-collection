using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RM_DialoguesLayout : RM_Layout {

    public Button dial1Btn, dial2Btn, dial3Btn;

	public void RM_ReturnClick(){
		Debug.Log("DialoguesReturn button clicked");
		SetLayouts(gm.introLayout);
		UnsetLayouts(gm.dialoguesLayout, gm.title);
	}

	public void RM_DialClick(int dial){
		Debug.Log("DialoguesDial button clicked");
		SetLayouts(gm.dialLayout);
		UnsetLayouts(gm.dialoguesLayout, gm.introTextObj);

		RM_DialLayout dialLayoutScript = gm.dialLayout.GetComponent(typeof (RM_DialLayout)) as RM_DialLayout;
		dialLayoutScript.activeDial = dial;
        // for the synthetiser to know it is an intro dial 
        dialLayoutScript.isDial = true;

		switch (dial)
        {
            case 1:
                dialLayoutScript.isMastico = gm.isMastico1;
                dialLayoutScript.pitch = gm.pitch1;
                dialLayoutScript.phonems = gm.introDial1;
                break;
            case 2:
                dialLayoutScript.isMastico = gm.isMastico2;
                dialLayoutScript.pitch = gm.pitch2;
                dialLayoutScript.phonems = gm.introDial2;
                break;
            case 3:
                dialLayoutScript.isMastico = gm.isMastico3;
                dialLayoutScript.pitch = gm.pitch3;
                dialLayoutScript.phonems = gm.introDial3;
                break;
            default: break;
        }
        dialLayoutScript.updateMasticoSprite();
        dialLayoutScript.GetDialText();
	}

    public void SetDialButtons(){
        // Enable dial buttons based on whether intro texts are set
        dial2Btn.interactable = !string.IsNullOrEmpty(gm.introText1);
        dial3Btn.interactable = !string.IsNullOrEmpty(gm.introText2);
    }
}
