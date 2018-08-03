angular.module('virtoCommerce.catalogModule')
    .directive('vaDictionaryProperty', function () {
        return {
            restrict: 'E',
            templateUrl: 'Modules/$(VirtoCommerce.Catalog)/Scripts/directives/dictionaryProperty.tpl.html',
            require: ['^form', 'ngModel'],
            scope: {
                ngModel: "=",
                validationRules: "="
            },
            replace: true,
            transclude: true
        }
    });