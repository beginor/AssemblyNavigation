using System.Collections.Generic;

namespace GridModule {

	public class Category {

		public string CategoryId {
			get;
			set;
		}

		public string CategoryName {
			get;
			set;
		}

		public string Description {
			get;
			set;
		}

	}

	public class CategoryCollection : List<Category> {

	}
}