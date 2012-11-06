using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WALT.DAL
{
    abstract public class Processor
    {
        protected Mediator _mediator;
        protected Database _db;

        public Processor(Mediator mediator)
        {
            _mediator = mediator;
            _db = mediator.GetDatabase();
        }
    }
}
