(() => {
	window.onscroll = scrollFunction;
	//docuemnt.getElementById("product-type").onchange = ...
})();
let slideIndex = 1;
showSlides(slideIndex);

function scrollFunction() {
	if (document.body.scrollTop > 80 || document.documentElement.scrollTop > 80) {
		document.getElementById("navbar").style.padding = "20px 10px 0 10";
		document.getElementById("logo").style.fontSize = "25px";
		document.getElementById("logo-img").style.height = "65px";
		document.getElementById("logo-img").style.width = "85px";
	} else {
			document.getElementById("logo").style.fontSize = "35px";
		document.getElementById("logo-img").style.height = "105px";
		document.getElementById("logo-img").style.width = "135px";
	}
}



function onTypeChange() {
	// var value = e.target.value;
	var value = document.querySelector("#product-type-select").value;
	var subtypeSelect = document.querySelector("#product-sub-type");

	while (subtypeSelect.children.length > 0)
		subtypeSelect.removeChild(subtypeSelect.children[0]);

	var none = document.createElement("option");
	none.innerText = "None";
	none.value = null;

	subtypeSelect.appendChild(none);

	switch (value) {
		case "Washing Machine":
			// var option = document.createElement("option");
			// option.value = "Dish Washer";
			// option.innerHTML = "Dish Washer";
			// subtypeSelect.appendChild(option);		

			CreateSubType("Flight Washer");
			CreateSubType("Bot Washer");
			CreateSubType("Undercounter Dish Washer");
			CreateSubType("Rack Conveyor Washer");
			CreateSubType("Hood Dish Washer");
			break;

		case "Hot Line":
			CreateSubType("Range");
			CreateSubType("Fry Top Grill");
			CreateSubType("Fryer");
			CreateSubType("Charcoal Grill");
			CreateSubType("Brat Pan");
			CreateSubType("Boiling Pan");
			CreateSubType("Combi Steamer Oven");
			CreateSubType("Convection Oven");
			CreateSubType("Bain Marei");
			CreateSubType("Salamander");
			break;

		case "Cold Line":
			CreateSubType("Freezer Room");
			CreateSubType("Vertical Refrigerator");
			CreateSubType("Undercounter Refrigerator");
			CreateSubType("Salad Refrigerator");
			CreateSubType("Pizza Refrigerator");
			CreateSubType("Ice Cream Refrigerator");
			CreateSubType("Ice Maker");
			CreateSubType("Sushi Overcounter Refrigerator");
			CreateSubType("Plast Chiller Shock Freezer");
			CreateSubType("Show Case Refrigerator");
			break;

		case "Bakery & Pastry":
			CreateSubType("Spiral Mixer");
			CreateSubType("Plantary Mixer");
			CreateSubType("Dough Divider Rounder");
			CreateSubType("Dough Moulder");
			CreateSubType("Dough Sheeter");
			CreateSubType("Rack Oven");
			CreateSubType("Deck oven");
			CreateSubType("Pizza Oven");
			CreateSubType("Proven");
			CreateSubType("Bread Slicer");
			break;

		case "Laundry":
			CreateSubType("Washer Extractor");
			CreateSubType("Drier");
			CreateSubType("Iron");
			CreateSubType("Flatwork Ironing");
			break;

		case "Preparation":
			CreateSubType("Meet Slicer");
			CreateSubType("Meet Mincer");
			CreateSubType("Meet Saw");
			CreateSubType("Sausage Filler");
			CreateSubType("Burger Press");
			CreateSubType("Kitchen Blinder");
			CreateSubType("Food Proccessor");
			CreateSubType("Potato Peeler");
			CreateSubType("Vegetable Cutter");
			CreateSubType("Vegetable Washer");
			CreateSubType("Other");
			break;

		case "Coffee & bar Equipment":
			CreateSubType("Coffee Machine");
			CreateSubType("Bar Blinder");
			CreateSubType("Juice Extractor");
			CreateSubType("Microwave");
			CreateSubType("Toaster");
			CreateSubType("Milk Shaker");
			break;

		case "buffet":

			break;
		case "others":
			break;
	}

	function CreateSubType(subTypeName) {
		var option = document.createElement("option");
		option.value = subTypeName;
		option.innerHTML = subTypeName;
		subtypeSelect.appendChild(option);
	}
}


// function AddNewProduct() {
// 	var productName = document.getElementById("product-name").value;
// 	var productType = document.getElementById("product-type").value;
// 	var productSubType = document.getElementById("product-sub-type").value;
// 	var productBuyPrice = document.getElementById("product-buy-price").value;
// 	var productSellPrice = document.getElementById("product-sell-price").innerHTML;
// 	var productCondition = document.getElementById("condition-selection").value;
// }

function onProductSubmit() {
	var prodForm = document.forms["insert-new-product-form"];
	var productInfo = {
		name: prodForm["product-name"].value,
		type: prodForm["product-type"].value,
		subType: prodForm["product-sub-type"].value,
		buyPrice: prodForm["product-buy-price"].value,
		sellPrice: prodForm["product-sell-price"].value,
		condition: prodForm["product-sell-price"].value,
	};
	console.log(productInfo);
}

// Sllide Show functions:

function plusSlides(n) {
	showSlides(slideIndex += n);
}

function showSlides(n) {
	let i;
	let slides = document.getElementsByClassName("slides-effect");
	let dots = document.getElementsByClassName("dot");
	if (n > slides.length)
		slideIndex = 1;
	if (n < 1)
		slideIndex = slides.length;

	for (i = 0; i < slides.length; i++) {
		slides[i].style.display = "none";
	}

	dots[slideIndex - 1].className = dots[slideIndex - 1].className.replace("active", ""); // not well understand

	slides[slideIndex - 1].style.display = "block";
	dots[slideIndex - 1].className += "active";

}

function currentSlides(n) {
	showSlides(slideIndex = n);
}


// json object for froducts on submit
// assign name attributes to form 