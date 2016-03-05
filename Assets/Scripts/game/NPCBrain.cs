using System.Collections;
using System.Collections.Generic;
using ThugLib;
using ThugSimpleGame;

namespace ThugSimpleGame {
    public abstract class NPCBrain {
        public Dictionary<string,object> parameters = new Dictionary<string,
           object>();
        public Dictionary<string,object> data = new Dictionary<string,object>();
        public bool active = true;
        public NPCScript body;

        public NPCBrain(NPCScript body)
        {
            this.body = body;
        }

        public abstract void Run();
    }
}
