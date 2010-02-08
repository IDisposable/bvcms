using System; 
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using System.ComponentModel;

namespace CmsData
{
	[Table(Name="disc.OtherNotify")]
	public partial class OtherNotify : INotifyPropertyChanging, INotifyPropertyChanged
	{
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
	#region Private Fields
		
		private int _Id;
		
		private string _Email;
		
		private int? _BlogId;
		
   		
    	
		private EntityRef< Blog> _Blog;
		
	#endregion
	
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
		
		partial void OnIdChanging(int value);
		partial void OnIdChanged();
		
		partial void OnEmailChanging(string value);
		partial void OnEmailChanged();
		
		partial void OnBlogIdChanging(int? value);
		partial void OnBlogIdChanged();
		
    #endregion
		public OtherNotify()
		{
			
			
			this._Blog = default(EntityRef< Blog>); 
			
			OnCreated();
		}

		
    #region Columns
		
		[Column(Name="Id", UpdateCheck=UpdateCheck.Never, Storage="_Id", DbType="int NOT NULL", IsPrimaryKey=true)]
		public int Id
		{
			get { return this._Id; }

			set
			{
				if (this._Id != value)
				{
				
                    this.OnIdChanging(value);
					this.SendPropertyChanging();
					this._Id = value;
					this.SendPropertyChanged("Id");
					this.OnIdChanged();
				}

			}

		}

		
		[Column(Name="email", UpdateCheck=UpdateCheck.Never, Storage="_Email", DbType="varchar(50)")]
		public string Email
		{
			get { return this._Email; }

			set
			{
				if (this._Email != value)
				{
				
                    this.OnEmailChanging(value);
					this.SendPropertyChanging();
					this._Email = value;
					this.SendPropertyChanged("Email");
					this.OnEmailChanged();
				}

			}

		}

		
		[Column(Name="BlogId", UpdateCheck=UpdateCheck.Never, Storage="_BlogId", DbType="int")]
		public int? BlogId
		{
			get { return this._BlogId; }

			set
			{
				if (this._BlogId != value)
				{
				
					if (this._Blog.HasLoadedOrAssignedValue)
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
				
                    this.OnBlogIdChanging(value);
					this.SendPropertyChanging();
					this._BlogId = value;
					this.SendPropertyChanged("BlogId");
					this.OnBlogIdChanged();
				}

			}

		}

		
    #endregion
        
    #region Foreign Key Tables
   		
	#endregion
	
	#region Foreign Keys
    	
		[Association(Name="FK_OtherNotify_Blog", Storage="_Blog", ThisKey="BlogId", IsForeignKey=true)]
		public Blog Blog
		{
			get { return this._Blog.Entity; }

			set
			{
				Blog previousValue = this._Blog.Entity;
				if (((previousValue != value) 
							|| (this._Blog.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if (previousValue != null)
					{
						this._Blog.Entity = null;
						previousValue.OtherNotifications.Remove(this);
					}

					this._Blog.Entity = value;
					if (value != null)
					{
						value.OtherNotifications.Add(this);
						
						this._BlogId = value.Id;
						
					}

					else
					{
						
						this._BlogId = default(int?);
						
					}

					this.SendPropertyChanged("Blog");
				}

			}

		}

		
	#endregion
	
		public event PropertyChangingEventHandler PropertyChanging;
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
				this.PropertyChanging(this, emptyChangingEventArgs);
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

   		
	}

}
