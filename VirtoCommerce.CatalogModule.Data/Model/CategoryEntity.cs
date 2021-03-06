using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Domain.Catalog.Model;
using System.Collections.Generic;

namespace VirtoCommerce.CatalogModule.Data.Model
{
    public class CategoryEntity : AuditableEntity
    {
        public CategoryEntity()
        {
            Images = new NullCollection<ImageEntity>();
            CategoryPropertyValues = new NullCollection<PropertyValueEntity>();
            OutgoingLinks = new NullCollection<CategoryRelationEntity>();
            IncommingLinks = new NullCollection<CategoryRelationEntity>();
            Properties = new NullCollection<PropertyEntity>();
        }

        [Required]
        [StringLength(64)]
        public string Code { get; set; }

        [Required]
        public bool IsActive { get; set; }

        public int Priority { get; set; }
        [Required]
        [StringLength(128)]
        public string Name { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [StringLength(64)]
        public string TaxType { get; set; }

        [NotMapped]
        public CategoryEntity[] AllParents
        {
            get
            {
                var retVal = new CategoryEntity[] { };
                if (ParentCategory != null)
                {
                    retVal = ParentCategory.AllParents.Concat(new[] { ParentCategory }).ToArray();
                }
                return retVal;
            }
        }

        #region Navigation Properties
        [StringLength(128)]
        [ForeignKey("Catalog")]
        [Required]
        public string CatalogId { get; set; }

        public virtual CatalogEntity Catalog { get; set; }

        [StringLength(128)]
        [ForeignKey("ParentCategory")]
        public string ParentCategoryId { get; set; }

        public virtual CategoryEntity ParentCategory { get; set; }

        public virtual ObservableCollection<ImageEntity> Images { get; set; }

        public virtual ObservableCollection<PropertyValueEntity> CategoryPropertyValues { get; set; }
        //It new navigation property for link replace to stupid CategoryLink (will be removed later)
        public virtual ObservableCollection<CategoryRelationEntity> OutgoingLinks { get; set; }
        public virtual ObservableCollection<CategoryRelationEntity> IncommingLinks { get; set; }
        public virtual ObservableCollection<PropertyEntity> Properties { get; set; }
        #endregion


        public virtual Category ToModel(Category category)
        {
            if (category == null)
            {
                throw new ArgumentNullException(nameof(category));
            }

            category.Id = Id;
            category.CreatedBy = CreatedBy;
            category.CreatedDate = CreatedDate;
            category.ModifiedBy = ModifiedBy;
            category.ModifiedDate = ModifiedDate;

            category.Code = Code;
            category.Name = Name;
            category.Priority = Priority;
            category.TaxType = TaxType;

            category.CatalogId = CatalogId;

            category.ParentId = ParentCategoryId;
            category.IsActive = IsActive;

            category.Links = OutgoingLinks.Select(x => x.ToModel(new CategoryLink())).ToList();
            category.Images = Images.OrderBy(x => x.SortOrder).Select(x => x.ToModel(AbstractTypeFactory<Image>.TryCreateInstance())).ToList();
            category.Properties = Properties.Select(x => x.ToModel(AbstractTypeFactory<Property>.TryCreateInstance())).ToList();

            //category property values
            category.PropertyValues = CategoryPropertyValues.OrderBy(x => x.Name).SelectMany(x => x.ToModel(AbstractTypeFactory<PropertyValue>.TryCreateInstance())).ToList();
            return category;
        }

        public virtual CategoryEntity FromModel(Category category, PrimaryKeyResolvingMap pkMap)
        {
            if (category == null)
            {
                throw new ArgumentNullException(nameof(category));
            }

            pkMap.AddPair(category, this);

            Id = category.Id;
            CreatedBy = category.CreatedBy;
            CreatedDate = category.CreatedDate;
            ModifiedBy = category.ModifiedBy;
            ModifiedDate = category.ModifiedDate;

            Code = category.Code;
            Name = category.Name;
            Priority = category.Priority;
            TaxType = category.TaxType;
            CatalogId = category.CatalogId;

            ParentCategoryId = category.ParentId;
            EndDate = DateTime.UtcNow.AddYears(100);
            StartDate = DateTime.UtcNow;
            IsActive = category.IsActive ?? true;

            if (category.PropertyValues != null)
            {
                CategoryPropertyValues = new ObservableCollection<PropertyValueEntity>(AbstractTypeFactory<PropertyValueEntity>.TryCreateInstance().FromModels(category.PropertyValues, pkMap));
            }

            if (category.Links != null)
            {
                OutgoingLinks = new ObservableCollection<CategoryRelationEntity>(category.Links.Select(x => AbstractTypeFactory<CategoryRelationEntity>.TryCreateInstance().FromModel(x)));
            }

            if (category.Images != null)
            {
                Images = new ObservableCollection<ImageEntity>(category.Images.Select(x => AbstractTypeFactory<ImageEntity>.TryCreateInstance().FromModel(x, pkMap)));
            }

            return this;
        }

        public virtual void Patch(CategoryEntity target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            target.CatalogId = CatalogId;
            target.ParentCategoryId = ParentCategoryId;
            target.Code = Code;
            target.Name = Name;
            target.TaxType = TaxType;
            target.Priority = Priority;
            target.IsActive = IsActive;

            if (!CategoryPropertyValues.IsNullCollection())
            {
                CategoryPropertyValues.Patch(target.CategoryPropertyValues, (sourcePropValue, targetPropValue) => sourcePropValue.Patch(targetPropValue));
            }

            if (!OutgoingLinks.IsNullCollection())
            {
                OutgoingLinks.Patch(target.OutgoingLinks, new LinkedCategoryComparer(), (sourceLink, targetLink) => sourceLink.Patch(targetLink));
            }

            if (!Images.IsNullCollection())
            {
                Images.Patch(target.Images, (sourceImage, targetImage) => sourceImage.Patch(targetImage));
            }

        }

    }
}
