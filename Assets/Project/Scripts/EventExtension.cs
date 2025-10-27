using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class EventExtensions
{
    public static void ActOnce(this Action actionEvent, Action handler)
    {
        Action wrapper = null;
        wrapper = () =>
        {
            handler();
            actionEvent -= wrapper;
        };
        actionEvent += wrapper;
    }
}
