﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio
{
    public  class Imagen
    {
        public Articulo IdArticulo { get; set; }
        public int IdImagen { get; set; }  
        public string URL { get; set; }
    }
}
