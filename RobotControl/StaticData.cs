using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotControl
{
    internal class StaticData
    {

        /// <summary>
        /// The standard module used for ABB Online Stream mode
        /// </summary>
        public static string[] StreamModule =
        {
            "MODULE StreamModule",
            "",
            "  PERS speeddata velocity:=[50,50,5000,1000];",
            "  PERS zonedata zone:=[FALSE,5,8,8,0.8,8,0.8];",
            "",
            "  PERS bool aborted := FALSE;",
            "  PERS num pnum := -1;",
            "",
            "  PERS bool pset0 := FALSE;",
            "  PERS bool pset1 := FALSE;",
            "  PERS bool pset2 := FALSE;",
            "  PERS bool pset3 := FALSE;",
            "",
            "  PERS jointtarget home1 := [[1,1,1,1,1,30],[9E9,9E9,9E9,9E9,9E9,9E9]];  ! small rots to avoid singularity problems",
            "",
            "  PERS robtarget p0 := [[377.22,4.21,546.99],[0,-0.0056,1,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]];",
            "  PERS robtarget p1 := [[260.33,4.21,671.48],[0,-0.0081,1,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]];",
            "  PERS robtarget p2 := [[128.75,4.21,562.63],[0,-0.0163,0.9999,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]];",
            "  PERS robtarget p3 := [[229.12,4.21,424.47],[0,-0.0092,1,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]];",
            "",
            "",
            "  PROC main()",
            "    AccSet 10, 10;",
            "    ConfL\\Off;",
            "    ConfJ\\Off;",
            "",
            "    MoveAbsJ home1,velocity,fine,Tool0\\WObj:=WObj0;",
            "    Path0;",
            "  ENDPROC",
            "",
            "  PROC Path0()",
            "    WHILE NOT aborted DO",
            "      WaitUntil pset0 = TRUE;",
            "      pset0 := FALSE;",
            "      pnum := 0;",
            "      MoveL p0,velocity,zone,Tool0\\WObj:=WObj0;",
            "",
            "      WaitUntil pset1 = TRUE;",
            "      pset1 := FALSE;",
            "      pnum := 1;",
            "      MoveL p1,velocity,zone,Tool0\\WObj:=WObj0;",
            "",
            "      WaitUntil pset2 = TRUE;",
            "      pset2 := FALSE;",
            "      pnum := 2;",
            "      MoveL p2,velocity,zone,Tool0\\WObj:=WObj0;",
            "",
            "      WaitUntil pset3 = TRUE;",
            "      pset3 := FALSE;",
            "      pnum := 3;",
            "      MoveL p3,velocity,zone,Tool0\\WObj:=WObj0;",
            "    ENDWHILE",
            "  ENDPROC",
            "",
            "ENDMODULE"
        };
    }


}
