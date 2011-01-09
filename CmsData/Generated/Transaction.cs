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
	[Table(Name="dbo.Transaction")]
	public partial class Transaction : INotifyPropertyChanging, INotifyPropertyChanged
	{
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
	#region Private Fields
		
		private int _Id;
		
		private DateTime? _TransactionDate;
		
		private string _TransactionGateway;
		
		private int? _DatumId;
		
		private bool? _Testing;
		
		private decimal? _Amt;
		
		private string _ApprovalCode;
		
		private bool? _Approved;
		
		private string _TransactionId;
		
		private string _Message;
		
		private string _AuthCode;
		
		private decimal? _Amtdue;
		
		private string _Url;
		
		private string _Description;
		
		private string _Name;
		
		private string _Address;
		
		private string _City;
		
		private string _State;
		
		private string _Zip;
		
		private string _Phone;
		
		private string _Emails;
		
		private string _Participants;
		
		private int? _OrgId;
		
		private int? _OriginalId;
		
   		
   		private EntitySet< TransactionPerson> _TransactionPeople;
		
    	
	#endregion
	
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
		
		partial void OnIdChanging(int value);
		partial void OnIdChanged();
		
		partial void OnTransactionDateChanging(DateTime? value);
		partial void OnTransactionDateChanged();
		
		partial void OnTransactionGatewayChanging(string value);
		partial void OnTransactionGatewayChanged();
		
		partial void OnDatumIdChanging(int? value);
		partial void OnDatumIdChanged();
		
		partial void OnTestingChanging(bool? value);
		partial void OnTestingChanged();
		
		partial void OnAmtChanging(decimal? value);
		partial void OnAmtChanged();
		
		partial void OnApprovalCodeChanging(string value);
		partial void OnApprovalCodeChanged();
		
		partial void OnApprovedChanging(bool? value);
		partial void OnApprovedChanged();
		
		partial void OnTransactionIdChanging(string value);
		partial void OnTransactionIdChanged();
		
		partial void OnMessageChanging(string value);
		partial void OnMessageChanged();
		
		partial void OnAuthCodeChanging(string value);
		partial void OnAuthCodeChanged();
		
		partial void OnAmtdueChanging(decimal? value);
		partial void OnAmtdueChanged();
		
		partial void OnUrlChanging(string value);
		partial void OnUrlChanged();
		
		partial void OnDescriptionChanging(string value);
		partial void OnDescriptionChanged();
		
		partial void OnNameChanging(string value);
		partial void OnNameChanged();
		
		partial void OnAddressChanging(string value);
		partial void OnAddressChanged();
		
		partial void OnCityChanging(string value);
		partial void OnCityChanged();
		
		partial void OnStateChanging(string value);
		partial void OnStateChanged();
		
		partial void OnZipChanging(string value);
		partial void OnZipChanged();
		
		partial void OnPhoneChanging(string value);
		partial void OnPhoneChanged();
		
		partial void OnEmailsChanging(string value);
		partial void OnEmailsChanged();
		
		partial void OnParticipantsChanging(string value);
		partial void OnParticipantsChanged();
		
		partial void OnOrgIdChanging(int? value);
		partial void OnOrgIdChanged();
		
		partial void OnOriginalIdChanging(int? value);
		partial void OnOriginalIdChanged();
		
    #endregion
		public Transaction()
		{
			
			this._TransactionPeople = new EntitySet< TransactionPerson>(new Action< TransactionPerson>(this.attach_TransactionPeople), new Action< TransactionPerson>(this.detach_TransactionPeople)); 
			
			
			OnCreated();
		}

		
    #region Columns
		
		[Column(Name="Id", UpdateCheck=UpdateCheck.Never, Storage="_Id", AutoSync=AutoSync.OnInsert, DbType="int NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
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

		
		[Column(Name="TransactionDate", UpdateCheck=UpdateCheck.Never, Storage="_TransactionDate", DbType="datetime")]
		public DateTime? TransactionDate
		{
			get { return this._TransactionDate; }

			set
			{
				if (this._TransactionDate != value)
				{
				
                    this.OnTransactionDateChanging(value);
					this.SendPropertyChanging();
					this._TransactionDate = value;
					this.SendPropertyChanged("TransactionDate");
					this.OnTransactionDateChanged();
				}

			}

		}

		
		[Column(Name="TransactionGateway", UpdateCheck=UpdateCheck.Never, Storage="_TransactionGateway", DbType="varchar(50)")]
		public string TransactionGateway
		{
			get { return this._TransactionGateway; }

			set
			{
				if (this._TransactionGateway != value)
				{
				
                    this.OnTransactionGatewayChanging(value);
					this.SendPropertyChanging();
					this._TransactionGateway = value;
					this.SendPropertyChanged("TransactionGateway");
					this.OnTransactionGatewayChanged();
				}

			}

		}

		
		[Column(Name="DatumId", UpdateCheck=UpdateCheck.Never, Storage="_DatumId", DbType="int")]
		public int? DatumId
		{
			get { return this._DatumId; }

			set
			{
				if (this._DatumId != value)
				{
				
                    this.OnDatumIdChanging(value);
					this.SendPropertyChanging();
					this._DatumId = value;
					this.SendPropertyChanged("DatumId");
					this.OnDatumIdChanged();
				}

			}

		}

		
		[Column(Name="testing", UpdateCheck=UpdateCheck.Never, Storage="_Testing", DbType="bit")]
		public bool? Testing
		{
			get { return this._Testing; }

			set
			{
				if (this._Testing != value)
				{
				
                    this.OnTestingChanging(value);
					this.SendPropertyChanging();
					this._Testing = value;
					this.SendPropertyChanged("Testing");
					this.OnTestingChanged();
				}

			}

		}

		
		[Column(Name="amt", UpdateCheck=UpdateCheck.Never, Storage="_Amt", DbType="money")]
		public decimal? Amt
		{
			get { return this._Amt; }

			set
			{
				if (this._Amt != value)
				{
				
                    this.OnAmtChanging(value);
					this.SendPropertyChanging();
					this._Amt = value;
					this.SendPropertyChanged("Amt");
					this.OnAmtChanged();
				}

			}

		}

		
		[Column(Name="ApprovalCode", UpdateCheck=UpdateCheck.Never, Storage="_ApprovalCode", DbType="varchar(50)")]
		public string ApprovalCode
		{
			get { return this._ApprovalCode; }

			set
			{
				if (this._ApprovalCode != value)
				{
				
                    this.OnApprovalCodeChanging(value);
					this.SendPropertyChanging();
					this._ApprovalCode = value;
					this.SendPropertyChanged("ApprovalCode");
					this.OnApprovalCodeChanged();
				}

			}

		}

		
		[Column(Name="Approved", UpdateCheck=UpdateCheck.Never, Storage="_Approved", DbType="bit")]
		public bool? Approved
		{
			get { return this._Approved; }

			set
			{
				if (this._Approved != value)
				{
				
                    this.OnApprovedChanging(value);
					this.SendPropertyChanging();
					this._Approved = value;
					this.SendPropertyChanged("Approved");
					this.OnApprovedChanged();
				}

			}

		}

		
		[Column(Name="TransactionId", UpdateCheck=UpdateCheck.Never, Storage="_TransactionId", DbType="varchar(50)")]
		public string TransactionId
		{
			get { return this._TransactionId; }

			set
			{
				if (this._TransactionId != value)
				{
				
                    this.OnTransactionIdChanging(value);
					this.SendPropertyChanging();
					this._TransactionId = value;
					this.SendPropertyChanged("TransactionId");
					this.OnTransactionIdChanged();
				}

			}

		}

		
		[Column(Name="Message", UpdateCheck=UpdateCheck.Never, Storage="_Message", DbType="varchar(50)")]
		public string Message
		{
			get { return this._Message; }

			set
			{
				if (this._Message != value)
				{
				
                    this.OnMessageChanging(value);
					this.SendPropertyChanging();
					this._Message = value;
					this.SendPropertyChanged("Message");
					this.OnMessageChanged();
				}

			}

		}

		
		[Column(Name="AuthCode", UpdateCheck=UpdateCheck.Never, Storage="_AuthCode", DbType="varchar(50)")]
		public string AuthCode
		{
			get { return this._AuthCode; }

			set
			{
				if (this._AuthCode != value)
				{
				
                    this.OnAuthCodeChanging(value);
					this.SendPropertyChanging();
					this._AuthCode = value;
					this.SendPropertyChanged("AuthCode");
					this.OnAuthCodeChanged();
				}

			}

		}

		
		[Column(Name="amtdue", UpdateCheck=UpdateCheck.Never, Storage="_Amtdue", DbType="money")]
		public decimal? Amtdue
		{
			get { return this._Amtdue; }

			set
			{
				if (this._Amtdue != value)
				{
				
                    this.OnAmtdueChanging(value);
					this.SendPropertyChanging();
					this._Amtdue = value;
					this.SendPropertyChanged("Amtdue");
					this.OnAmtdueChanged();
				}

			}

		}

		
		[Column(Name="URL", UpdateCheck=UpdateCheck.Never, Storage="_Url", DbType="varchar(80)")]
		public string Url
		{
			get { return this._Url; }

			set
			{
				if (this._Url != value)
				{
				
                    this.OnUrlChanging(value);
					this.SendPropertyChanging();
					this._Url = value;
					this.SendPropertyChanged("Url");
					this.OnUrlChanged();
				}

			}

		}

		
		[Column(Name="Description", UpdateCheck=UpdateCheck.Never, Storage="_Description", DbType="varchar(80)")]
		public string Description
		{
			get { return this._Description; }

			set
			{
				if (this._Description != value)
				{
				
                    this.OnDescriptionChanging(value);
					this.SendPropertyChanging();
					this._Description = value;
					this.SendPropertyChanged("Description");
					this.OnDescriptionChanged();
				}

			}

		}

		
		[Column(Name="Name", UpdateCheck=UpdateCheck.Never, Storage="_Name", DbType="varchar(100)")]
		public string Name
		{
			get { return this._Name; }

			set
			{
				if (this._Name != value)
				{
				
                    this.OnNameChanging(value);
					this.SendPropertyChanging();
					this._Name = value;
					this.SendPropertyChanged("Name");
					this.OnNameChanged();
				}

			}

		}

		
		[Column(Name="Address", UpdateCheck=UpdateCheck.Never, Storage="_Address", DbType="varchar(50)")]
		public string Address
		{
			get { return this._Address; }

			set
			{
				if (this._Address != value)
				{
				
                    this.OnAddressChanging(value);
					this.SendPropertyChanging();
					this._Address = value;
					this.SendPropertyChanged("Address");
					this.OnAddressChanged();
				}

			}

		}

		
		[Column(Name="City", UpdateCheck=UpdateCheck.Never, Storage="_City", DbType="varchar(50)")]
		public string City
		{
			get { return this._City; }

			set
			{
				if (this._City != value)
				{
				
                    this.OnCityChanging(value);
					this.SendPropertyChanging();
					this._City = value;
					this.SendPropertyChanged("City");
					this.OnCityChanged();
				}

			}

		}

		
		[Column(Name="State", UpdateCheck=UpdateCheck.Never, Storage="_State", DbType="varchar(4)")]
		public string State
		{
			get { return this._State; }

			set
			{
				if (this._State != value)
				{
				
                    this.OnStateChanging(value);
					this.SendPropertyChanging();
					this._State = value;
					this.SendPropertyChanged("State");
					this.OnStateChanged();
				}

			}

		}

		
		[Column(Name="Zip", UpdateCheck=UpdateCheck.Never, Storage="_Zip", DbType="varchar(15)")]
		public string Zip
		{
			get { return this._Zip; }

			set
			{
				if (this._Zip != value)
				{
				
                    this.OnZipChanging(value);
					this.SendPropertyChanging();
					this._Zip = value;
					this.SendPropertyChanged("Zip");
					this.OnZipChanged();
				}

			}

		}

		
		[Column(Name="Phone", UpdateCheck=UpdateCheck.Never, Storage="_Phone", DbType="varchar(15)")]
		public string Phone
		{
			get { return this._Phone; }

			set
			{
				if (this._Phone != value)
				{
				
                    this.OnPhoneChanging(value);
					this.SendPropertyChanging();
					this._Phone = value;
					this.SendPropertyChanged("Phone");
					this.OnPhoneChanged();
				}

			}

		}

		
		[Column(Name="Emails", UpdateCheck=UpdateCheck.Never, Storage="_Emails", DbType="varchar(400)")]
		public string Emails
		{
			get { return this._Emails; }

			set
			{
				if (this._Emails != value)
				{
				
                    this.OnEmailsChanging(value);
					this.SendPropertyChanging();
					this._Emails = value;
					this.SendPropertyChanged("Emails");
					this.OnEmailsChanged();
				}

			}

		}

		
		[Column(Name="Participants", UpdateCheck=UpdateCheck.Never, Storage="_Participants", DbType="varchar")]
		public string Participants
		{
			get { return this._Participants; }

			set
			{
				if (this._Participants != value)
				{
				
                    this.OnParticipantsChanging(value);
					this.SendPropertyChanging();
					this._Participants = value;
					this.SendPropertyChanged("Participants");
					this.OnParticipantsChanged();
				}

			}

		}

		
		[Column(Name="OrgId", UpdateCheck=UpdateCheck.Never, Storage="_OrgId", DbType="int")]
		public int? OrgId
		{
			get { return this._OrgId; }

			set
			{
				if (this._OrgId != value)
				{
				
                    this.OnOrgIdChanging(value);
					this.SendPropertyChanging();
					this._OrgId = value;
					this.SendPropertyChanged("OrgId");
					this.OnOrgIdChanged();
				}

			}

		}

		
		[Column(Name="OriginalId", UpdateCheck=UpdateCheck.Never, Storage="_OriginalId", DbType="int")]
		public int? OriginalId
		{
			get { return this._OriginalId; }

			set
			{
				if (this._OriginalId != value)
				{
				
                    this.OnOriginalIdChanging(value);
					this.SendPropertyChanging();
					this._OriginalId = value;
					this.SendPropertyChanged("OriginalId");
					this.OnOriginalIdChanged();
				}

			}

		}

		
    #endregion
        
    #region Foreign Key Tables
   		
   		[Association(Name="FK_TransactionPeople_Transaction", Storage="_TransactionPeople", OtherKey="Id")]
   		public EntitySet< TransactionPerson> TransactionPeople
   		{
   		    get { return this._TransactionPeople; }

			set	{ this._TransactionPeople.Assign(value); }

   		}

		
	#endregion
	
	#region Foreign Keys
    	
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

   		
		private void attach_TransactionPeople(TransactionPerson entity)
		{
			this.SendPropertyChanging();
			entity.Transaction = this;
		}

		private void detach_TransactionPeople(TransactionPerson entity)
		{
			this.SendPropertyChanging();
			entity.Transaction = null;
		}

		
	}

}
