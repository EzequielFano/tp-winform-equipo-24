﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Dominio;
using Microsoft.Win32;

namespace Negocio
{
    public class ArticulosNegocio
    {
        public List<Articulo> TraerListado()
        {
            List<Articulo> articulos = new List<Articulo>();
            AccesoDatos datos = new AccesoDatos();

            try
            {
                datos.setearConsulta("select A.Id, A.Codigo, A.Nombre, A.Descripcion, M.Descripcion Marca,A.IdMarca,A.IdCategoria, C.Descripcion Categoria,A.Precio, IM.ImagenUrl from ARTICULOS A INNER join IMAGENES IM ON A.Id= IM.IdArticulo INNER JOIN MARCAS M ON A.IdMarca = M.Id LEFT JOIN CATEGORIAS C ON A.IdCategoria = C.Id\r\n");
                datos.ejecutarLectura();
                while (datos.Lector.Read())
                {
                    Articulo aux = new Articulo();
                    aux.IdArticulo = datos.Lector.GetInt32(0);
                    aux.CodigoArticulo = (string)datos.Lector["Codigo"];
                    aux.NombreArticulo = (string)datos.Lector["Nombre"];
                    aux.Descripcion = (string)datos.Lector["Descripcion"];
                    aux.Marca = new Marca();
                    aux.Marca.Descripcion = (string)datos.Lector["Marca"];
                    aux.Marca.Id = (int)datos.Lector["IdMarca"];
                    aux.Categoria = new Categoria();
                    if (datos.Lector["Categoria"] is DBNull)
                    {
                        aux.Categoria = null;
                    }
                    else
                    {
                        aux.Categoria = new Categoria();
                        aux.Categoria.Descripcion = (string)datos.Lector["Categoria"];
                        aux.Categoria.Id = (int)datos.Lector["IdCategoria"];
                    }
                    aux.Precio = (int)datos.Lector.GetSqlMoney(8);
                    aux.URLImagen= new Imagen();
                    if (!(datos.Lector["ImagenUrl"] is DBNull))
                    {
                        aux.URLImagen.URL = (string)datos.Lector["ImagenUrl"];
                    }
                    articulos.Add(aux);

                }
                return articulos;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
        public void agregarArticulo(Articulo articulo)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                ImagenNegocio imagenNegocio = new ImagenNegocio();
                Imagen imag = new Imagen();

                datos.setearConsulta("insert into ARTICULOS (Codigo,Nombre,Descripcion,IdMarca,IdCategoria,Precio) values (@Codigo,@Nombre,@Descripcion,@IdMarca,@IdCategoria,@Precio)");
                datos.setearParametro("@Codigo", articulo.CodigoArticulo);
                datos.setearParametro("@Nombre", articulo.NombreArticulo);
                datos.setearParametro("@Descripcion", articulo.Descripcion);
                datos.setearParametro("@IdMarca", articulo.Marca.Id);
                datos.setearParametro("@IdCategoria", articulo.Categoria.Id);
                datos.setearParametro("@Precio", articulo.Precio);
                datos.ejecutarAccion();
                imag = imagenNegocio.ObtenerIDarticuloCargado();
                datos.setearConsulta("INSERT INTO IMAGENES (IdArticulo,ImagenUrl) VALUES (@IdArticulo, @ImagenUrl)");
                datos.setearParametro("@IdArticulo", imag.IdImagen);
                datos.setearParametro("@ImagenUrl", articulo.URLImagen.URL);
                datos.ejecutarAccion();
            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {
                datos.cerrarConexion();
            }
                         
        }
        public void modificarArticulo(Articulo articulo)
        {
            AccesoDatos datos = new AccesoDatos();
            ImagenNegocio imagenNegocio = new ImagenNegocio();
            Imagen imag = new Imagen();
            try
            {
                datos.setearConsulta("UPDATE ARTICULOS SET Codigo = @Codigo, Nombre = @Nombre, Descripcion = @Descripcion, IdMarca = @IdMarca, IdCategoria = @IdCategoria, Precio = @Precio WHERE Id = @IdArticulo");
                datos.setearParametro("IdArticulo", articulo.IdArticulo);
                datos.setearParametro("@Codigo", articulo.CodigoArticulo);
                datos.setearParametro("@Nombre", articulo.NombreArticulo);
                datos.setearParametro("@Descripcion", articulo.Descripcion);
                datos.setearParametro("@IdMarca", articulo.Marca.Id);
                datos.setearParametro("@IdCategoria", articulo.Categoria.Id);
                datos.setearParametro("@Precio", articulo.Precio);
                datos.ejecutarAccion();
               
                datos.setearConsulta("UPDATE IMAGENES SET ImagenUrl = @ImagenUrl WHERE IdArticulo = @Id");
                datos.setearParametro("@Id", articulo.IdArticulo);
                datos.setearParametro("@ImagenUrl", articulo.URLImagen.URL);
                datos.ejecutarAccion();
                
               
            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally { datos.cerrarConexion(); }
        }

        public void eliminarArticulo (int id)
        {
            AccesoDatos datos= new AccesoDatos ();  
            try
            {
                datos.setearConsulta("DELETE FROM ARTICULOS where id = @IdArticulo");
                datos.setearParametro("@IdArticulo", id);
                datos.ejecutarAccion();

            }
            catch (Exception ex )
            {

                throw ex;
            }
        }

        public List <Articulo> filtrar(string campo, string criterio, string filtro)
        {
            List <Articulo> lista = new List<Articulo> ();
            AccesoDatos datos = new AccesoDatos ();
            try
            {
                string consulta = "SELECT A.Id, A.Codigo, A.Nombre, A.Descripcion, M.Descripcion AS Marca, A.IdCategoria, A.IdMarca, C.Descripcion AS Categoria, A.Precio, IM.ImagenUrl\r\nFROM ARTICULOS A\r\nINNER JOIN IMAGENES IM ON A.Id = IM.IdArticulo\r\nINNER JOIN MARCAS M ON A.IdMarca = M.Id\r\nLEFT JOIN CATEGORIAS C ON A.IdCategoria = C.Id\r\nWHERE ";
                if (campo == "Categoria")
                {
                    consulta += "C.Descripcion = '" + criterio + "'";
                }
                else if(campo == "Marca")
                {
                    consulta += "M.Descripcion = '" + criterio + "'";
                }
                else 
                {
                    switch (criterio)
                    {
                        case "Comienza con":
                            consulta += "A.Nombre like '" + filtro + "%'";
                            break;
                        case "Termina con":
                            consulta += "A.Nombre like '%" + filtro + "'";
                            break;
                        case "Contiene":
                            consulta += "A.Nombre like '%" + filtro + "%'";
                            break;
                    }
                }




                datos.setearConsulta(consulta);
                datos.ejecutarLectura();
                while (datos.Lector.Read())
                {
                    Articulo aux = new Articulo();
                    aux.IdArticulo = datos.Lector.GetInt32(0);
                    aux.CodigoArticulo = (string)datos.Lector["Codigo"];
                    aux.NombreArticulo = (string)datos.Lector["Nombre"];
                    aux.Descripcion = (string)datos.Lector["Descripcion"];
                    aux.Marca = new Marca();
                    aux.Marca.Descripcion = (string)datos.Lector["Marca"];
                    aux.Marca.Id = (int)datos.Lector["IdMarca"];
                    aux.Categoria = new Categoria();
                    aux.Categoria.Descripcion = (string)datos.Lector["Categoria"];
                    aux.Categoria.Id = (int)datos.Lector["IdCategoria"];
                    aux.Precio = (int)datos.Lector.GetSqlMoney(8);
                    aux.URLImagen = new Imagen();
                    if (!(datos.Lector["ImagenUrl"] is DBNull))
                    {
                        aux.URLImagen.URL = (string)datos.Lector["ImagenUrl"];
                    }
                    lista.Add(aux);

                }
                return lista; 
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }

}
