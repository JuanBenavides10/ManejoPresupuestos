function inicializarFormularioTransacciones(urlObtenerCategorias) {
	$("#TipoOperacionId").change(async function () { //     // evento que se activa cuando cambiamos de valor el <select> TipoOperacionId

		const valorSeleccionado = $(this).val();    // $(this) se refiere al <select> que lanzó el evento y .val() obtiene el valor seleccionado (el "value" del <option>)
		const respuesta = await fetch(urlObtenerCategorias, { //fetch permite solicitudes HTTP asíncronas al backend
			method: 'POST', // hace un POST al backend
			body: valorSeleccionado, // manda el valor seleccionado como cuerpo de la petición
			headers: { //le decimos al servidor qué tipo de datos envíamos (JSON).
				'Content-Type': 'application/json'
			}
		});
		const json = await respuesta.json(); //Convierte la respuesta del backend (JSON) a un objeto JavaScript.
		console.log(json);
		const opciones = json.map(categoria => `<option value=${categoria.value}>${categoria.text}</option>`); //Recorre las categorías y construye un string de <option> por cada una
		$("#CategoriaId").html(opciones); //Reemplaza TODO el contenido del <select id="CategoriaId">  con las nuevas opciones generadas
	})
}