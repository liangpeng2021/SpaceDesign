using SpaceDesign;
using UnityEngine;

namespace KartGame.KartSystems
{

    public class KeyboardInput : BaseInput
    {
        public string TurnInputName = "Horizontal";
        public string AccelerateButtonName = "Accelerate";
        public string BrakeButtonName = "Brake";


        public override InputData GenerateInput()
        {
            return new InputData
            {
                Accelerate = KartingCPE.Inst.Accelerate,
                Brake = KartingCPE.Inst.Brake,
                TurnInput = KartingCPE.Inst.TurnInput
            };
        }
        //public override InputData GenerateInput() {
        //    return new InputData
        //    {
        //        Accelerate = Input.GetButton(AccelerateButtonName),
        //        Brake = Input.GetButton(BrakeButtonName),
        //        TurnInput = Input.GetAxis("Horizontal")
        //    };
        //}
    }
}
