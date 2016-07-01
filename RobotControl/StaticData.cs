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
            //"  ! Table collision prevention",
            //"  VAR wzstationary table_box;",
            //"",
            "  PERS bool aborted := FALSE;",
            "  PERS num pnum := -1;",
            "",
            "  PERS speeddata vel0:=[20,20,1000,1000];",
            "  PERS speeddata vel1:=[20,20,1000,1000];",
            "  PERS speeddata vel2:=[20,20,1000,1000];",
            "  PERS speeddata vel3:=[20,20,1000,1000];",
            "",
            "  PERS zonedata zone0:=[FALSE,5,8,8,0.8,8,0.8];",
            "  PERS zonedata zone1:=[FALSE,5,8,8,0.8,8,0.8];",
            "  PERS zonedata zone2:=[FALSE,5,8,8,0.8,8,0.8];",
            "  PERS zonedata zone3:=[FALSE,5,8,8,0.8,8,0.8];",
            "",
            "  PERS bool pset0 := FALSE;",
            "  PERS bool pset1 := FALSE;",
            "  PERS bool pset2 := FALSE;",
            "  PERS bool pset3 := FALSE;",
            "",
            
            "  PERS robtarget p0 := [[377.22,4.21,546.99],[0,-0.0056,1,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]];",
            "  PERS robtarget p1 := [[260.33,4.21,671.48],[0,-0.0081,1,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]];",
            "  PERS robtarget p2 := [[128.75,4.21,562.63],[0,-0.0163,0.9999,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]];",
            "  PERS robtarget p3 := [[229.12,4.21,424.47],[0,-0.0092,1,0],[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]];",
            "",
            "  PERS jointtarget home1 := [[1,1,1,1,1,30],[9E9,9E9,9E9,9E9,9E9,9E9]];  ! small rots to avoid singularity problems",
            "",
            "  PROC main()",
            "    AccSet 10, 10;",
            "    ConfL\\Off;",
            "    ConfJ\\Off;",
            //"    wzone_power_on;",
            "",
            "    ! MoveAbsJ home1,vel0,zone0,Tool0\\WObj:=WObj0;",
            "    Path0;",
            "  ENDPROC",
            "",
            "  PROC Path0()",
            "    WHILE NOT aborted DO",
            "      WaitUntil pset0 = TRUE;",
            "      pset0 := FALSE;",
            "      pnum := 0;",
            "      MoveL p0,vel0,zone0,Tool0\\WObj:=WObj0;",
            "",
            "      WaitUntil pset1 = TRUE;",
            "      pset1 := FALSE;",
            "      pnum := 1;",
            "      MoveL p1,vel1,zone1,Tool0\\WObj:=WObj0;",
            "",
            "      WaitUntil pset2 = TRUE;",
            "      pset2 := FALSE;",
            "      pnum := 2;",
            "      MoveL p2,vel2,zone2,Tool0\\WObj:=WObj0;",
            "",
            "      WaitUntil pset3 = TRUE;",
            "      pset3 := FALSE;",
            "      pnum := 3;",
            "      MoveL p3,vel3,zone3,Tool0\\WObj:=WObj0;",
            "    ENDWHILE",
            "  ENDPROC",
            "",
            //"  PROC wzone_power_on()",
            //"    VAR shapedata volume;",
            //"    CONST pos table_low:=[-1000, -1000, -100];",
            //"    CONST pos table_high:=[1000, 1000, 25];",
            //"    WZBoxDef \\Inside, volume, table_low, table_high;",
            //"    WZLimSup \\Stat, table_box, volume;",
            //"  ENDPROC",
            //"",
            "ENDMODULE"
        };
    }

}
